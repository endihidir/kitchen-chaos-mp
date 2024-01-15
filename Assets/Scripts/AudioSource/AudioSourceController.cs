using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioSourceController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    public AudioSource AudioSource => _audioSource;

    public SoundType soundType;
    
    public List<AudioSourceController> audioSourceControllers;

    private CancellationTokenSource _cancellationTokenSource;
    public async void DestroyWait(float duration = 0f)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), DelayType.DeltaTime, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        Destroy();
    }

    public void Destroy()
    {
        DisposeToken();
        audioSourceControllers?.Remove(this);
        Destroy(gameObject);
    }

    private void DisposeToken()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private void OnDestroy()
    {
        DisposeToken();
    }
}
