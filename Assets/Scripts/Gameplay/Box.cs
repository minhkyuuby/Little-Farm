using UnityEngine;
using System;
using System.Collections;

public class Box : MonoBehaviour, IWorldClickable
{
    [SerializeField] private Animation _animation;
    [SerializeField] private string _openAnimationName = "BoxOpen";
    [SerializeField, Min(0f)] private float _openFallbackDuration = 0.8f;
    [SerializeField] private bool _hideAfterOpen = true;

    private bool _isOpening;
    private bool _isOpened;

    public event Action<Box> Opened;

    public bool IsOpening => _isOpening;
    public bool IsOpened => _isOpened;

    private void OnValidate()
    {
        if (_animation == null)
        {
            _animation = GetComponent<Animation>();
        }
    }

    public bool TryOpen()
    {
        if (_isOpening || _isOpened)
        {
            return false;
        }

        StartCoroutine(OpenRoutine());
        return true;
    }

    public void ResetBoxState(bool closedVisible)
    {
        _isOpening = false;
        _isOpened = false;

        if (_animation != null)
        {
            _animation.Stop();
        }

        gameObject.SetActive(closedVisible);
    }

    public bool TryHandleWorldClick(WorldClickContext context)
    {
        Debug.Log($"Box clicked. Context: {context}");
        return TryOpen();
    }

    private IEnumerator OpenRoutine()
    {
        _isOpening = true;
        var waitSeconds = Mathf.Max(0f, _openFallbackDuration);

        if (_animation != null)
        {
            AnimationState chosenState = null;

            if (!string.IsNullOrWhiteSpace(_openAnimationName) && _animation[_openAnimationName] != null)
            {
                chosenState = _animation[_openAnimationName];
                _animation.Play(_openAnimationName);
            }
            else if (_animation.clip != null)
            {
                chosenState = _animation[_animation.clip.name];
                _animation.Play();
            }

            if (chosenState != null)
            {
                waitSeconds = Mathf.Max(0f, chosenState.length);
            }
        }

        if (waitSeconds > 0f)
        {
            yield return new WaitForSeconds(waitSeconds);
        }

        _isOpening = false;
        _isOpened = true;
        Opened?.Invoke(this);

        if (_hideAfterOpen)
        {
            gameObject.SetActive(false);
        }
    }
}
