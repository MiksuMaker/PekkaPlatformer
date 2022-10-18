using System;
using System.Collections;
using UnityEngine;

public class AnimatorHelper : MonoBehaviour {
       
    public static IEnumerator CheckAnimationCompleted(Animator _animator, string _animationName, Action Oncomplete, float _triggerCompleteAtNormalizedTime = 1f, int _animatorLayerIndex = 0)
    {
        yield return new WaitForSeconds(2f);
        AnimatorStateInfo animStateInfo = _animator.GetCurrentAnimatorStateInfo(_animatorLayerIndex);

        while (animStateInfo.IsName(_animationName) && !_animator.IsInTransition(_animatorLayerIndex) && animStateInfo.normalizedTime < _triggerCompleteAtNormalizedTime)
        {
            animStateInfo = _animator.GetCurrentAnimatorStateInfo(0);            
            yield return null;
        }
        
        if (Oncomplete != null)
            Oncomplete();
    }
}
