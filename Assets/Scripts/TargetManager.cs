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
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace HoloBlaster {
    /// <summary>
    /// Class that manages spawning and placement of targets into the environment
    /// </summary>
    public class TargetManager : Singleton<TargetManager> {
        [SerializeField]
        public ObjectPool markerPool;

        [SerializeField]
        public LayerMask layerMask;

        [Min(0.0f)]
        [SerializeField]
        public float MinSpawnThreshold = 0.5f;
        [SerializeField]
        public float MaxSpawnThreshold = 2.0f;
        [SerializeField]
        public float MinDistanceSpace = 0.5f;

        private const float MIN_RAYCAST_DISTANCE = 0.3f;
        private const float MAX_RAYCAST_DISTANCE = 10.0f;
        private const float HIT_OFFSET = 0.05f;

        private float markerTimer;
        private float markerTimeLimit;

        private float minDistanceSpaceSqr;

        // Prevent Non-singleton construction
        protected TargetManager() { }

        private void Awake() {
            Debug.Assert(MinSpawnThreshold < MaxSpawnThreshold, "MinSpawnThreshold is not less than MaxSpawnThreshold");

            markerTimeLimit = Random.Range(MinSpawnThreshold, MaxSpawnThreshold);

            minDistanceSpaceSqr = MinDistanceSpace * MinDistanceSpace;
        }

        private void Update() {
            markerTimer += Time.deltaTime;

            if (markerTimer > markerTimeLimit) {
                markerTimer = 0.0f;
                PlaceMarker();
            }
        }

        private bool IsValidPlacement(Vector3 pos) {
            foreach (var target in markerPool.ActiveObjects) {
                if (WithinDistance(target.GetGameObject().transform.position, pos, minDistanceSpaceSqr)) {
                    return false;
                }
            }

            return true;
        }

        private static bool WithinDistance(Vector3 p1, Vector3 p2, float sqrDistance) {
            return (p1 - p2).sqrMagnitude < sqrDistance;
        }

        private static Vector3 GenerateRandomDirection(Transform transform) {
            var topDownForward = new Vector3(transform.forward.x, 0.0f, transform.forward.z);
            var topDownRight = new Vector3(transform.right.x, 0.0f, transform.right.z);

            // Generate random raycast in 180 degree FOV of camera
            var raycastDir = Vector3.Lerp(topDownForward - topDownRight, topDownForward + topDownRight, Random.Range(0.0f, 1.0f));
            raycastDir = Vector3.Lerp(raycastDir - Vector3.up, raycastDir + Vector3.up, Random.Range(0.0f, 1.0f));

            return raycastDir;
        }

        private void PlaceMarker() {
            if (markerPool.HasAvailable()) {
                var cam = CameraCache.Main.transform;

                if (Physics.Raycast(
                        cam.position,
                        GenerateRandomDirection(cam),
                        out RaycastHit hit,
                        MAX_RAYCAST_DISTANCE,
                        layerMask)) {
                    if (hit.distance > MIN_RAYCAST_DISTANCE) {
                        var placementPoint = hit.point + hit.normal * HIT_OFFSET;

                        if (IsValidPlacement(placementPoint)) {
                            Vector3 gazeDirection = Camera.main.transform.forward;
                            Quaternion rotation = Quaternion.LookRotation(gazeDirection);

                            PlaceTarget(markerPool.Request(),
                                placementPoint,
                                rotation);
                        }
                    }
                }
            }
        }

        private void PlaceTarget(IObjectPoolItem item, Vector3 placementPoint, Quaternion rotation) {
            var targetObj = item.GetGameObject();
            targetObj.transform.position = placementPoint;
            targetObj.transform.rotation = rotation;

            var target = targetObj.GetComponentInChildren<Target>(true);
            if (target != null) {
                target.Lock();
            }
        }
    }
}