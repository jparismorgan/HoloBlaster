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
using System.Collections.Generic;
using UnityEngine;

namespace HoloBlaster {
    public class ObjectPool : MonoBehaviour {
        public GameObject Prefab;

        [SerializeField]
        private int objectPoolSize = 10;

        public int PoolSize {
            get => objectPoolSize;
        }

        public IReadOnlyCollection<IObjectPoolItem> ActiveObjects {
            get => activeObjects;
        }

        public IReadOnlyCollection<IObjectPoolItem> AvailableObjects {
            get => availableObjects;
        }

        private HashSet<IObjectPoolItem> activeObjects = new HashSet<IObjectPoolItem>();
        private Queue<IObjectPoolItem> availableObjects = new Queue<IObjectPoolItem>();

        public bool HasAvailable() {
            return availableObjects.Count > 0;
        }

        private void Awake() {
            for (int i = 0; i < PoolSize; i++) {
                var gameObject = Instantiate(Prefab, transform);
                var instance = gameObject.GetComponent<IObjectPoolItem>();
                if (instance != null) {
                    instance.RegisterPool(this);
                    availableObjects.Enqueue(instance);
                } else {
                    Debug.LogError($"{gameObject} does not implement required interface {typeof(IObjectPoolItem).Name}");
                    Destroy(gameObject);
                }
            }
        }

        public IObjectPoolItem Request() {
            if (!HasAvailable()) {
                return null;
            }

            var result = availableObjects.Dequeue();

            result.OnRequest();

            activeObjects.Add(result);

            return result;
        }

        public void RecycleObject(IObjectPoolItem returnObject) {
            if (activeObjects.Contains(returnObject)) {
                activeObjects.Remove(returnObject);

                returnObject.OnRecycle();

                availableObjects.Enqueue(returnObject);
            }
        }
    }
}