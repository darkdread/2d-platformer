using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausableRigidbody2D {

    private class Data {

        private Vector3 _velocity;
        private float _angularVelocity;

        
        public Data(Vector3 velocity, float angularVelocity) {
            _velocity = velocity;
            _angularVelocity = angularVelocity;
        }

        public Vector3 Velocity {
            get {
                return _velocity;
            }
        }

        public float AngularVelocity {
            get {
                return _angularVelocity;
            }
        }
    }

    private static Dictionary<Rigidbody2D, Data> rigidbody2DList = new Dictionary<Rigidbody2D, Data>();

    public static void PauseRigidbody(Rigidbody2D rb) {
        if (!rigidbody2DList.ContainsKey(rb)) {
            Data data = new Data(rb.velocity, rb.angularVelocity);
            
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;

            rigidbody2DList.Add(rb, data);
        }
    }

    public static void ResumeRigidbody(Rigidbody2D rb) {
        if (rigidbody2DList.ContainsKey(rb)) {
            Data data = rigidbody2DList[rb];
            rb.isKinematic = false;
            rb.velocity = data.Velocity;
            rb.angularVelocity = data.AngularVelocity;
            
            rigidbody2DList.Remove(rb);
        }
    }
}
