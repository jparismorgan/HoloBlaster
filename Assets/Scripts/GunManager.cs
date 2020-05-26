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
using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace HoloBlaster {
    /// <summary>
    /// Class that manages moving and shooting the guns
    /// </summary>
    public class GunManager : MonoBehaviour,
        IMixedRealityHandJointHandler // handle joint position updates for hands
    {
        /// <summary>
        /// Class that manages fields for a hand
        /// </summary>
        [Serializable]
        public class HandObject {
            [SerializeField]
            public GameObject gun;

            internal bool hasReleasedTrigger = false;
            internal GameObject shootFromHere;
        }

        enum TriggerConfiguration {
            Pointer,
            Thumb
        }

        // Pointer constants
#if UNITY_EDITOR
        private const float pointerTriggerThreshold = 0.08f; // how close the fingers have to be to shoot
        private const float pointerReleaseThreshold = 0.1f; // how far away the fingers have to be to release
#else
        private const float pointerTriggerThreshold = 0.05f;
        private const float pointerReleaseThreshold = 0.09f;
#endif

        // Thumb constants
#if UNITY_EDITOR
        private const float thumbTriggerThreshold = 0.08f; // how close the fingers have to be to shoot
        private const float thumbReleaseThreshold = 0.1f; // how far away the fingers have to be to release
#else
        private const float thumbTriggerThreshold = 0.042f;
        private const float thumbReleaseThreshold = 0.055f;
#endif

        private const float fireballCooldown = 0.1f;
        private const float transitionSpeed = 20.0f;

        [SerializeField]
        public HandObject leftHand;
        [SerializeField]
        public HandObject rightHand;

        [SerializeField]
        public GameObject bulletPrefab;
        [SerializeField]
        public AudioClip gunSound;

        private TriggerConfiguration triggerConfiguration = TriggerConfiguration.Pointer;
        private AudioSource audioSource;
        private Logger logger { get { return Logger.Instance; } } // Use a getter to avoid race conditions

        void Start() {
            rightHand.shootFromHere = rightHand.gun.transform.Find("Cylinder").gameObject.transform.Find("ShootFromHere").gameObject;
            leftHand.shootFromHere = leftHand.gun.transform.Find("Cylinder").gameObject.transform.Find("ShootFromHere").gameObject;
            audioSource = GetComponent<AudioSource>();
        }

        void Update() {
        }

        private void OnEnable() {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
        }

        private void OnDisable() {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHandJointHandler>(this);
        }

        public void activatePointerTriggerConfiguration() {
            logger.Log($"activatePointerTriggerConfiguration");
            triggerConfiguration = TriggerConfiguration.Pointer;
        }

        public void activateThumbTriggerConfiguration() {
            logger.Log($"activateThumbTriggerConfiguration");
            triggerConfiguration = TriggerConfiguration.Thumb;
        }

        public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData) {
            Handedness handedness = eventData.Handedness;
            HandObject hand = handedness == Handedness.Right ? rightHand : leftHand;
            if (!hand.gun.activeInHierarchy) {
                return;
            }

            // Move guns
            if (eventData.InputData.TryGetValue(TrackedHandJoint.Wrist, out MixedRealityPose wrist)) {
                Quaternion gunRotation = handedness == Handedness.Left ? wrist.Rotation * Quaternion.Euler(5, -5, -90) : wrist.Rotation * Quaternion.Euler(5, 5, 90);
                hand.gun.transform.position = Vector3.Lerp(hand.gun.transform.position, wrist.Position, Time.deltaTime * transitionSpeed);
                hand.gun.transform.rotation = gunRotation;
            }

            // Pointer finger
            if (triggerConfiguration == TriggerConfiguration.Pointer) {
                if (eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out MixedRealityPose indexTipPose) && eventData.InputData.TryGetValue(TrackedHandJoint.ThumbProximalJoint, out MixedRealityPose thumbProximalJoint)) {
                    float diff = (indexTipPose.Position - thumbProximalJoint.Position).magnitude;
                    logger.Log($"diff: {diff}");

                    // Has the trigger been squeezed?
                    if (hand.hasReleasedTrigger && diff < pointerTriggerThreshold) {
                        hand.hasReleasedTrigger = false;
                        Shoot(hand.shootFromHere.transform.position, wrist.Rotation);
                    }

                    // Has the trigger been released?
                    if (!hand.hasReleasedTrigger && diff > pointerReleaseThreshold) {
                        hand.hasReleasedTrigger = true;
                    }
                }
            } else if (triggerConfiguration == TriggerConfiguration.Thumb) {
                if (eventData.InputData.TryGetValue(TrackedHandJoint.IndexKnuckle, out MixedRealityPose indexKnucklePose) && eventData.InputData.TryGetValue(TrackedHandJoint.ThumbTip, out MixedRealityPose thumbTipPose)) {
                    float diff = (thumbTipPose.Position - indexKnucklePose.Position).magnitude;
                    logger.Log($"diff: {diff}");

                    // Has the trigger been squeezed?
                    if (hand.hasReleasedTrigger && diff < thumbTriggerThreshold) {
                        hand.hasReleasedTrigger = false;
                        Shoot(hand.shootFromHere.transform.position, wrist.Rotation);
                    }

                    // Has the trigger been released?
                    if (!hand.hasReleasedTrigger && diff > thumbReleaseThreshold) {
                        hand.hasReleasedTrigger = true;
                    }
                }
            }

        }

        private void Shoot(Vector3 origin, Quaternion rotation, Color? color = null) {
            logger.Log($"Shoot - origin: {origin} rotation: {rotation}");
            GameObject bullet = Instantiate(bulletPrefab, origin, rotation);
            if (color != null) {
                bullet.gameObject.GetComponent<Renderer>().material.SetColor("_Color", color.Value);
            }
            audioSource.PlayOneShot(gunSound, 0.2f);
        }
    }
}