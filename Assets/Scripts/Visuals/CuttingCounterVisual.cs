using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounterVisual : ProgressCounterVisual
{
    [SerializeField] private Animator _animator;

    private static readonly int Cut = Animator.StringToHash("Cut");

    private float _animTimer;

    private CancellationTokenSource _animationTokenSource;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        _animator = GetComponentInChildren<Animator>(true);
    }
#endif

    protected override void Start()
    {
        base.Start();

        _interactableCounter.OnComplete(KillAnimTokenClientRpc);
    }

    protected override void OnDropOrSwap()
    {
        base.OnDropOrSwap();

        _animationTokenSource = new CancellationTokenSource();
        
        TriggerAnimationClientRpc();
    }
    
    [ClientRpc]
    private void TriggerAnimationClientRpc()
    {
        TriggerAnimationAsync();
    }

    protected override void TakeObject()
    {
        base.TakeObject();
        
        KillAnimTokenClientRpc();
    }

    [ClientRpc]
    private void KillAnimTokenClientRpc()
    {
        KillAnimToken();
    }

    private async void TriggerAnimationAsync()
    {
        _animationTokenSource = new CancellationTokenSource();
        
        while (_animationTokenSource is not null)
        {
            _animTimer += Time.deltaTime;

            if (_animTimer > 0)
            {
                _animTimer = -0.12f;

                _animator.SetTrigger(Cut);
            
                SoundManager.Instance.PlaySound(SoundType.Chop, 0.1f);
            }

            try
            {
                await UniTask.Yield(_animationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Knife anim cancelled");
                return;
            }
        }
    }

    private void KillAnimToken()
    {
        _animTimer = 0f;
        _animationTokenSource?.Cancel();
        _animationTokenSource?.Dispose();
        _animationTokenSource = null;
    }
}