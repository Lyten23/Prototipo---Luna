using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class DirectionBiasController : MonoBehaviour
    {
        [Header("References")]
        public SpriteRenderer sprite;

        [SerializeField] private float timeRotation;

        private void FixedUpdate()
        {
            Turn();
        }

        void Turn()
        {
            if (sprite.flipX)
            {
                LeanTween.rotateY(gameObject, 180f, timeRotation).setEase(LeanTweenType.easeInOutSine);
            }
            else
            {
                LeanTween.rotateY(gameObject, 0f, timeRotation).setEase(LeanTweenType.easeInOutSine);
            }
        }
    }

}
