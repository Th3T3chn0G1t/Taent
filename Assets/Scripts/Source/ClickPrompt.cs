using System;
using Hazel;

namespace Taent {
	public class ClickPrompt : Entity {
        public void OnPickup() {
            GetComponent<AudioComponent>().Play();
        }

		protected override void OnCreate() {
        }

		protected override void OnUpdate(float ts) {}
	}
}
