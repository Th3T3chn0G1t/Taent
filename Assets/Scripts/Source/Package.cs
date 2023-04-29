using System;
using Hazel;

namespace Taent {
	public class Package : Entity {
        public Entity m_Player;
        private CharacterController m_PlayerInstance;

        public Entity m_PlayerTextOverlay;
        public string m_Text; 

        public string m_Delivery;

        private TextComponent m_PlayerTextOverlayText;

        public Entity m_Bin;
        public Entity m_BinLitterA;
        public Entity m_BinLitterB;
        public Entity m_BinLitterC;

        public Entity m_LightA;
        public Entity m_LightB;

        public Entity m_Wall;

        public Entity m_Package999;

        private static int DeliveryID = 0;
        private static bool ThreeActivated = false;

        public void OnPickup() {
            m_PlayerTextOverlayText.Text = m_Text;
            Log.Info("Delivery " + DeliveryID + " initiated! " + ThreeActivated);
            if(DeliveryID == 2) {
                GetComponent<AudioComponent>().SetEvent("Whispers");
                GetComponent<AudioComponent>().Play();
            }
            if(DeliveryID == 3 && !ThreeActivated) {
                m_LightA.GetComponent<PointLightComponent>().Radiance = new Vector3(1.0f, 0.0f, 0.0f);
                m_LightB.GetComponent<PointLightComponent>().Radiance = new Vector3(1.0f, 0.0f, 0.0f);

                m_PlayerInstance.NewSound("Talk");

                ThreeActivated = true;
            }
        }

        public void OnDrop() {
            GetComponent<AudioComponent>().Stop();
            m_PlayerTextOverlayText.Text = "";
        }

        public void OnDeliver() {
            m_PlayerInstance.Drop();

            GetComponent<AudioComponent>().Stop();
            if(DeliveryID != 2 && DeliveryID != 3 && DeliveryID != 4) {
                Log.Debug("Playing package audio event " + "Delivery_" + DeliveryID);
                GetComponent<AudioComponent>().SetEvent("Delivery_" + DeliveryID);
                GetComponent<AudioComponent>().Play();
            }

            switch(DeliveryID) {
                case 1: {
                    if(m_Delivery != "201") {
                        Vector3 r = m_Bin.Transform.Rotation;
                        r.Z += 30 * Mathf.Deg2Rad;
                        m_Bin.Transform.Rotation = r;
                        m_BinLitterA.Transform.Translation = new Vector3(-0.5f, 1.0f, 13.6f);
                        m_BinLitterB.Transform.Translation = new Vector3(-0.7f, 1.0f, 13.8f);
                        m_BinLitterC.Transform.Translation = new Vector3(-0.75f, 1.0f, 13.1f);
                    }
                    break;
                }
                case 2: {
                    break;
                }
                case 3: {
                    m_Wall.Destroy();
                    m_Package999.Transform.Translation = new Vector3(-10.4f, 0.4f, 6.0f);
                    GetComponent<AudioComponent>().SetEvent("Delivery_1");
                    GetComponent<AudioComponent>().Play();
                    break;
                }
                case 4: {
                    m_PlayerInstance.NewSound("Delivery_1");
                    m_LightA.GetComponent<PointLightComponent>().Radiance = new Vector3(0.0f, 0.0f, 0.0f);
                    m_LightB.GetComponent<PointLightComponent>().Radiance = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                }
            }

            DeliveryID++;

            Destroy();
        }

		protected override void OnCreate() {
            m_PlayerTextOverlayText = m_PlayerTextOverlay.GetComponent<TextComponent>();
            m_PlayerInstance = (CharacterController) m_Player.GetComponent<ScriptComponent>().Instance;
        }

		protected override void OnUpdate(float ts) {}
	}
}
