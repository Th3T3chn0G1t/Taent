// SPDX-License-Identifier: MIT
// Copyright (C) 2023 Emily "TTG" Banerjee <prs.ttg+taent@pm.me>

using System;
using Hazel;

namespace Taent {
	public class CharacterController : Entity {
        public float MouseSensitivity = 0.005f;
        public float MovementForce = 50.0f;
        public float JumpForce = 50.0f;
        public float AnchorForce = 10.0f;
        public float AnchorEpsilon = 1.0f;
        public float AnchorMax = 5.0f;
        public float MaxSpeed = 5.0f;
        public float OnGroundEpsilonBuffer = 0.0f;
        public float PickupTorque = 5.0f;

		private RigidBodyComponent m_RigidBody;
        private RaycastData m_FloorRaycastData;

		public Entity m_CameraEntity;
		private TransformComponent m_CameraTransform;
		private Vector2 m_LastMousePosition;

        public Vector3 m_Checkpoint;

        private bool m_WasOnGround = false;
        private bool m_DidJump = false;

        private RaycastData m_PickupRaycastData;
        private Entity m_HeldEntity;
        public Entity m_Anchor;
		private TransformComponent m_AnchorTransform;

        private class HeldSettings {
            public float m_LinearDrag;
            public float m_AngularDrag;
        }
        private HeldSettings m_HeldSettings = new HeldSettings();

		protected override void OnCreate() {
            m_RigidBody = GetComponent<RigidBodyComponent>();

			m_CameraTransform = m_CameraEntity.GetComponent<TransformComponent>();
			m_LastMousePosition = Input.GetMousePosition();

			Input.SetCursorMode(CursorMode.Locked);

			m_PickupRaycastData = new RaycastData();
			m_PickupRaycastData.MaxDistance = AnchorMax;
			m_PickupRaycastData.RequiredComponents = new[] { typeof(RigidBodyComponent) };
            m_PickupRaycastData.ExcludedEntities = new[] { ID };

            m_FloorRaycastData = new RaycastData();
            m_FloorRaycastData.MaxDistance = 2 * GetComponent<CapsuleColliderComponent>().HalfHeight + float.Epsilon + OnGroundEpsilonBuffer;
			m_FloorRaycastData.RequiredComponents = new[] { typeof(StaticMeshComponent) };
            m_FloorRaycastData.ExcludedEntities = new[] { ID };
			
            m_AnchorTransform = m_Anchor.GetComponent<TransformComponent>();
		}

        protected override void OnPhysicsUpdate(float ts) {
            m_FloorRaycastData.Origin = Translation;
            m_FloorRaycastData.Direction = Vector3.Down;

            bool onGround = false;
            if(Physics.CastRay(m_FloorRaycastData, out var hitInfo)) onGround = true;

            if(onGround && m_DidJump && !m_WasOnGround) m_DidJump = false;

            int keyHash = -1;

            float forward = 0.0f;
            float right = 0.0f;
            if(Input.IsKeyDown(KeyCode.W)) {
                forward = 1.0f;
                keyHash += 1;
            }
            else if(Input.IsKeyDown(KeyCode.S)) {
                forward = -1.0f;
                keyHash += 2;
            }
            
            if(Input.IsKeyDown(KeyCode.A)) {
                right = -1.0f;
                keyHash += 4;
            }
            else if(Input.IsKeyDown(KeyCode.D)) {
                right = 1.0f;
                keyHash += 8;
            }

            Vector3 flatForward = m_CameraEntity.Transform.WorldTransform.Forward;
            flatForward.Y = 0;
            Vector3 flatRight = m_CameraEntity.Transform.WorldTransform.Right;

            m_RigidBody.LinearVelocity = new Vector3(0.0f, m_RigidBody.LinearVelocity.Y, 0.0f);

            // TODO: Going down slopes is still floaty
            if(!m_DidJump && !onGround && m_RigidBody.LinearVelocity.Y > float.Epsilon) {
                m_RigidBody.LinearVelocity = new Vector3(m_RigidBody.LinearVelocity.X, 0.0f, m_RigidBody.LinearVelocity.Z);
            }

            Vector3 planarVelocity = m_RigidBody.LinearVelocity;
            planarVelocity.Y = 0;

            // TODO: onGround branch for air control
            if(planarVelocity.Length() < MaxSpeed) {
                m_RigidBody.AddForce((flatForward * forward + flatRight * right).Normalized() * MovementForce, EForceMode.VelocityChange);
            }

            if(Input.IsKeyPressed(KeyCode.Space) && onGround) {
                m_DidJump = true;
                m_RigidBody.AddForce(Vector3.Up * JumpForce, EForceMode.Impulse);
            }

            m_WasOnGround = onGround;
        }

        public void NewSound(string id) {
            GetComponent<AudioComponent>().Stop();
            GetComponent<AudioComponent>().SetEvent(id);
            GetComponent<AudioComponent>().Play();
        }

        public void Drop() {
            if(m_HeldEntity) {
                RigidBodyComponent rb = m_HeldEntity.GetComponent<RigidBodyComponent>();
                rb.LinearDrag = m_HeldSettings.m_LinearDrag;
                rb.AngularDrag = m_HeldSettings.m_AngularDrag;

                if(m_HeldEntity.HasComponent<ScriptComponent>()) {
                    object instance = m_HeldEntity.GetComponent<ScriptComponent>().Instance;
                    if(instance is Package) {
                        ((Package) instance).OnDrop();
                    }
                }


                m_HeldEntity = null;
            }
        }
        
		protected override void OnUpdate(float ts) {
            {
                Vector2 currentMousePosition = Input.GetMousePosition();
                Vector2 delta = (m_LastMousePosition - currentMousePosition) * ts;

                Vector3 rotation = m_CameraTransform.Rotation + new Vector3(delta.Y, delta.X, 0.0f) * MouseSensitivity;
                rotation.X = Math.Clamp(rotation.X, -90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad);
                m_CameraTransform.Rotation = rotation;

                m_LastMousePosition = currentMousePosition;
            }
            
            {
                if(Input.IsMouseButtonPressed(MouseButton.Left)) {

                    if(m_HeldEntity) Drop();
                    else {
                        m_PickupRaycastData.Origin = m_CameraTransform.WorldTransform.Position;
                        m_PickupRaycastData.Direction = m_CameraTransform.WorldTransform.Forward;

                        if(Physics.CastRay(m_PickupRaycastData, out var hitInfo)) {
                            Log.Debug("Hit entity " + hitInfo.Entity.Tag);
                            RigidBodyComponent rb = hitInfo.Entity.GetComponent<RigidBodyComponent>();
                            if(hitInfo.Entity.Tag != "Player") {
                                bool skip = false;
                                if(hitInfo.Entity.HasComponent<ScriptComponent>()) {
                                    object instance = hitInfo.Entity.GetComponent<ScriptComponent>().Instance;
                                    if(instance is Package) {
                                        ((Package) instance).OnPickup();
                                    }
                                    else if(instance is ClickPrompt) {
                                        ((ClickPrompt) instance).OnPickup();
                                        skip = true;
                                    }
                                }

                                if(rb.BodyType == EBodyType.Dynamic && !skip) {
                                    m_HeldEntity = hitInfo.Entity;
                                    m_HeldSettings.m_LinearDrag = rb.LinearDrag;
                                    m_HeldSettings.m_AngularDrag = rb.AngularDrag;
                                    rb.LinearDrag = 10.0f;
                                    rb.AngularDrag = 0.1f;
                                    rb.AddTorque(Hazel.Random.Vec3() * PickupTorque);
                                }
                            }
                        }
                    }
                }

                if(m_HeldEntity) {
                    Vector3 delta = m_AnchorTransform.WorldTransform.Position - m_HeldEntity.GetComponent<TransformComponent>().Translation;
                    if(delta.Length() > AnchorEpsilon) {
                        if(delta.Length() > AnchorMax) {
                            RigidBodyComponent rb = m_HeldEntity.GetComponent<RigidBodyComponent>();
                            rb.LinearDrag = m_HeldSettings.m_LinearDrag;
                            rb.AngularDrag = m_HeldSettings.m_AngularDrag;

                            m_HeldEntity = null;
                        }
                        else {
                            m_HeldEntity.GetComponent<RigidBodyComponent>().AddForce(delta * AnchorForce * ts, EForceMode.Impulse);
                        }
                    }
                }
            }
		}
	}
}
