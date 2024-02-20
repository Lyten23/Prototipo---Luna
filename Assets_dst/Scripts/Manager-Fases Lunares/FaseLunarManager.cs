using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class FaseLunarManager : MonoBehaviour
    {
        public enum EPhase
        {
            Phase1,
            Phase2,
        }
        [SerializeField] private PlayerController player;

        [SerializeField] private Camera trackedPlayerCamera;

        [SerializeField] private Transform phase1Anchor;

        [field:SerializeField] private Transform phase2Anchor;

        public EPhase currentPhas { get; private set; } = EPhase.Phase1;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
