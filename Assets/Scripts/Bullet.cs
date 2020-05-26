/***************************************************************************
 *   This file is part of the HoloBlaster project                          *
 *   Copyright (C) 2020 by J Paris Morgan                                  *
 *                                                                         *
 **                   GNU General Public License Usage                    **
 *                                                                         *
 *   This library is free software: you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation, either version 3 of the License, or     *
 *   (at your option) any later version.                                   *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *                                                                         *
 *   This library is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 ****************************************************************************/
using UnityEngine;

namespace HoloBlaster {
    public class Bullet : MonoBehaviour {
        private const float speed = 3f;

        public GameObject explosionPrefab;

        void Start() {
            Destroy(this.gameObject, 8.0f);
        }

        void Update() {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.tag != "FlareGun" && collision.gameObject.tag != "UI") {
                ContactPoint contact = collision.contacts[0];
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 position = contact.point;

                GameObject explosionObject = Instantiate(explosionPrefab, position, rotation) as GameObject;
                Destroy(explosionObject, 4.0f);

                Destroy(this.gameObject);
            }
        }
    }
}