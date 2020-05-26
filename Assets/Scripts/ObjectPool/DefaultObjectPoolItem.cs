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

namespace HoloBlaster {
    public class DefaultObjectPoolItem : MonoBehaviour, IObjectPoolItem {
        #region IObjectPoolItem interface
        private ObjectPool myPool;

        public void RegisterPool(ObjectPool pool) {
            myPool = pool;
        }

        public void OnRequest() {
            gameObject.SetActive(true);
        }

        public void OnRecycle() {
            gameObject.SetActive(false);
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        #endregion

        public void Reset() {
            myPool.RecycleObject(this);
        }

        private void Awake() {
            gameObject.SetActive(false);
        }
    }
}