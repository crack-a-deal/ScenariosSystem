using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animation kran;

    public void KranAnimation()
    {
        kran.Play();
    }

    public void PlayAnimation(string animationName)
    {
        kran.Play(animationName);
    }
}
