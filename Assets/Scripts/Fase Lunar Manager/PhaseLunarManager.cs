using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FaseCharacter
{
    CC,
    LL,
}

namespace PlayerController
{
    public class PhaseManager : MonoBehaviour
    {
        [Header("Referencias")] public FaseCharacter currentPhase;
        [SerializeField] Animator animatorLl;
        [SerializeField] Animator animatorCc;
        [SerializeField] GameObject characterLL;
        [SerializeField] GameObject characterCC;
        [SerializeField] ScriptableStats statsLl;
        [SerializeField] ScriptableStats statsCC;
        [Header("PhaseSettings")] public int prueba;
        [Header("Phase Timing")] public float phaseChangeInterval = 5f;
        private float phaseChangeTimer;
        [SerializeField] private bool stopChangingPhases = false;
        [SerializeField] private bool stop;

        void Start()
        {
            ChangedPhase(FaseCharacter.CC);
            phaseChangeTimer = phaseChangeInterval;
        }

        void Update()
        {
            if (!stopChangingPhases)
            {
                phaseChangeTimer -= Time.deltaTime;
                if (phaseChangeTimer <= 0)
                {
                    ChangePhaseTimer();
                    phaseChangeTimer = phaseChangeInterval;
                }
            }

            /*if (playerController.playerInput.actions["Pruebas"].WasPerformedThisFrame())
            {
                stop = true;
                CheckConditionAndChangePhase(stop);
            }*/
        }

        public void ChangedPhase(FaseCharacter newPhase)
        {
            currentPhase = newPhase;
            switch (currentPhase)
            {
                case FaseCharacter.LL:
                    characterLL.SetActive(true);
                    characterCC.SetActive(false);
                    break;
                case FaseCharacter.CC:
                    characterCC.SetActive(true);
                    characterLL.SetActive(false);
                    break;
            }
        }

        private void ChangePhaseTimer()
        {
            if (currentPhase == FaseCharacter.LL)
            {
                ChangedPhase(FaseCharacter.CC);
            }
            else
            {
                ChangedPhase(FaseCharacter.LL);
            }
        }

        public void ChangeToSpecificPhase(FaseCharacter specificPhase)
        {
            ChangedPhase(specificPhase);
            stopChangingPhases = true; // Detener los cambios automáticos de fase
        }

        public void CheckConditionAndChangePhase(bool condition)
        {
            if (condition)
            {
                ChangeToSpecificPhase(FaseCharacter.LL); // Cambia a la fase específica si se cumple la condición
            }
        }
    }
}
