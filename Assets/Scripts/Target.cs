/***************************************************************************
 *   This file is part of the HoloBlaster project                          *
 *   This file came from XR-BREAK and was modified, as noted in the LICENSE*
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
using UnityEngine.Events;
using UnityEngine.XR.WSA;

namespace HoloBlaster {
    public class Target : MonoBehaviour {
        // IMPORTANT - This code assumes Target script is always a direct child of the root of the prefab.

        private readonly Vector3 axis = new Vector3(0, 1, 0); // which axis it will rotate. x,y or z.
        private const float angle = 0.1f; // Angle covered per update - the speed

        [Header("References")]
        [SerializeField]
        public DefaultObjectPoolItem PoolItem;
        [SerializeField]
        public AudioClip destroyedSound;
        [SerializeField]
        public GameObject explosionPrefab;

        [Header("Lifetime")]
        [SerializeField]
        public bool AutoDestruct = true;
        [SerializeField]
        public float Lifetime = 15f;

        [Header("Events")]
        public UnityEvent OnSpawn = new UnityEvent();
        public UnityEvent OnCapture = new UnityEvent();
        public UnityEvent OnRelease = new UnityEvent();

        protected bool active = false;
        private float timer = 0.0f;

        private WorldAnchor anchor = null;
        private AudioSource targetManagerAudio;
        private Logger logger;

        void Start() {
            logger = Logger.Instance;
            targetManagerAudio = GameObject.Find("TargetManager").GetComponent<AudioSource>();
        }

        void Update() {
            // Rotate the target
            transform.RotateAround(transform.position, axis, angle);

            // Should we destroy the target?
            if (active && AutoDestruct) {
                timer += Time.deltaTime;
                if (timer > Lifetime) {
                    Release();
                }
            }
        }

        public void Lock() {
            if (anchor == null) {
                anchor = transform.parent.gameObject.AddComponent<WorldAnchor>();
            }
        }

        public void Unlock() {
            if (anchor != null) {
                Destroy(anchor);
                anchor = null;
            }
        }

        protected virtual void OnEnable() {
            active = true;
            timer = 0.0f;

            OnSpawn?.Invoke();
        }

        protected virtual void OnDisable() {
            active = false;
            Unlock();
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.tag != "FlareGun" && collision.gameObject.tag != "UI") {
                if (active) {
                    logger.Log($"OnCollisionEnter - Target with {collision.contacts[0].ToString()}");
                    ContactPoint contact = collision.contacts[0];
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
                    Vector3 position = contact.point;

                    GameObject explosionObject = Instantiate(explosionPrefab, position, rotation) as GameObject;
                    Destroy(explosionObject, 4.0f);

                    targetManagerAudio.PlayOneShot(destroyedSound, 0.8f);

                    OnCapture?.Invoke();
                    Release();
                }
            }
        }

        protected virtual void Release() {
            active = false;
            OnRelease?.Invoke();

            // Release ourselves back to the object pool
            PoolItem.Reset();
        }
    }
}