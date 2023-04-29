using System;
using Hazel;

namespace Taent {
	public class Checkpoint : Entity {
		public Scene m_EndScene;

		private float m_CounterDev = 0.0f;
		private float m_Counter = 0.0f;

		private void OnTrigger(Entity other) {
			if(other.HasComponent<ScriptComponent>()) {
				object instance = other.GetComponent<ScriptComponent>().Instance;
				if(instance is Package && ((Package) instance).m_Delivery == GetComponent<TagComponent>().Tag) {
					((Package) instance).OnDeliver();
					if(Tag == "-999") {
						Transform.Translation = new Vector3(0.0f, 100.0f, 0.0f);
						m_CounterDev = 1.0f;
					}
				}
			}
		}

		protected override void OnCreate() {
			TriggerBeginEvent += OnTrigger;
		}

		protected override void OnUpdate(float ts) {
			m_Counter += m_CounterDev;
			if(m_Counter > 100.0f) Application.Quit();
		}
	}
}
