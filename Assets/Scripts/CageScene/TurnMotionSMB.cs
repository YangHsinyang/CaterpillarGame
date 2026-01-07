using UnityEngine;

public class TurnMotionSMB : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = true;
        //Debug.Log($"applyRootMotion = {animator.applyRootMotion}");

        var ctrl = animator.GetComponentInParent<CaterpillarMover>();
        if (ctrl != null)
            ctrl.SetTurningByAnim(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Nothing required. With applyRootMotion==true, Unity applies the clip�fs root motion automatically.
        // Keep this method empty unless you need per-frame hooks.
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Capture the rotation applied by root motion before disabling it
        Quaternion currentRotation = animator.transform.rotation;

        var ctrl = animator.GetComponentInParent<CaterpillarMover>();
        if (ctrl != null)
        {
            ctrl.ApplyRotation(currentRotation);
            ctrl.SetTurningByAnim(false);
        }

        animator.applyRootMotion = false;

    }
}
