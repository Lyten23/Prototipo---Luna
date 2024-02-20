using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class PhaseLunarManager : MonoBehaviour
    {
        public enum EPhase
        {
            Phase1,
            Phase2,
        }

        [SerializeField] private PlayerController player;
        [SerializeField] private Camera cam;
        [SerializeField] private Transform phase1Anchor;
        [SerializeField] private Transform phase2Anchor;
        [SerializeField] private GameObject phase1GO;
        [SerializeField] private GameObject phase2GO;
        [field: SerializeField] public EPhase currentPhase { get; private set; } = EPhase.Phase1;
        public Vector3 currentAnchorPosition => currentPhase == EPhase.Phase1 ? phase1Anchor.position : phase2Anchor.position;

        public Vector3 otherAnchorPosition => currentPhase == EPhase.Phase1 ? phase2Anchor.position : phase1Anchor.position;
        void Start()
        {
            
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                FlipWorlds();
                Debug.Log("Quiero Cambiar de fase!!!");
            }
            Vector3 playerOffset=player.transform.position - currentAnchorPosition;
            Vector3 newShadowPlayerLocation = playerOffset + otherAnchorPosition;
        }

        void FlipWorlds()
        {
            //Todo - Safety Check
            currentPhase = currentPhase == EPhase.Phase1 ? EPhase.Phase2 : EPhase.Phase1;
            if (currentPhase==EPhase.Phase1)
            {
                phase1GO.SetActive(true);
                phase2GO.SetActive(false);
            }else if (currentPhase==EPhase.Phase2)
            {
                phase2GO.SetActive(true);
                phase1GO.SetActive(false);
            }
        }
    }
}
