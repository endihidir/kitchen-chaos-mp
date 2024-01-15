using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public abstract class ProgressCounterVisual : BaseCounterVisual
{
    [SerializeField] private Image _barImage;

    private Transform _barHandler;

    private float _progressTime, _progressTimer;
    
    private CancellationTokenSource _cancellationTokenSource;

    protected NetworkVariable<float> _progressBarValue = new NetworkVariable<float>(0);
    
    protected override void Awake()
    {
        base.Awake();
        
        _barHandler = _barImage.transform.parent;
        _barHandler.gameObject.SetActive(false);
        _barHandler.forward = Camera.main.transform.forward;
    }

    protected override void Start()
    {
        base.Start();
        
        _interactableCounter.OnStart(OnDropOrSwap).OnInterrupt(TakeObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _progressBarValue.OnValueChanged += OnProgressBarValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        _progressBarValue.OnValueChanged -= OnProgressBarValueChanged;
    }

    private void OnProgressBarValueChanged(float previousvalue, float newvalue)
    {
        SetBarFillAmount(newvalue);
    }

    protected virtual void OnDropOrSwap()
    {
        OnDropOrSwapClientRpc();
    }

    [ClientRpc]
    private void OnDropOrSwapClientRpc()
    {
        _progressTimer = 0f;
        SetProgressBarValue(_progressTimer);
        _progressTime = _interactableCounter.ProgressDuration;
        _barHandler.gameObject.SetActive(true);
        
        DisposeCancellationToken();
        _cancellationTokenSource = new CancellationTokenSource();

        WaitProgress();
    }

    private async void WaitProgress()
    {
        while (_progressTimer < _progressTime && _cancellationTokenSource is not null)
        {
            _progressTimer += Time.deltaTime;

            var barVal = _progressTimer / _progressTime;
            
            SetProgressBarValue(Mathf.Clamp01(barVal));

            try
            {
                await UniTask.Delay(1, DelayType.DeltaTime, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Visual progress task was cancelled! : " + name);
                return;
            }
        }
    }

    protected virtual void TakeObject()
    {
        TakeObjectClientRpc();
    }

    [ClientRpc]
    private void TakeObjectClientRpc()
    {
        DisposeCancellationToken();
        _barHandler.gameObject.SetActive(false);
        _progressTimer = 0f;
        SetProgressBarValue(_progressTimer);
    }

    private void SetProgressBarValue(float value)
    {
        if (IsServer)
        {
            _progressBarValue.Value = value;
        }
    }

    private void DisposeCancellationToken()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
    
    protected void SetBarFillAmount(float val) => _barImage.fillAmount = val;
    public override void OnDestroy()
    {
        base.OnDestroy();
        DisposeCancellationToken();
    }
}