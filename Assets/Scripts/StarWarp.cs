using UnityEngine;

// Randomly changes the position of the star on the title screen menu
public class StarWarp : StateMachineBehaviour
{
    Transform star;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!star)
            star = animator.transform.Find("Star");

        star.localPosition = new Vector2(Random.Range(-100, 901), Random.Range(-480, 480));
    }
}
