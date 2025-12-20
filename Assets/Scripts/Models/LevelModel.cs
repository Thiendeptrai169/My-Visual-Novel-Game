using UnityEngine;

[System.Serializable] // Can be adjust in the Inspector
public class LevelModel
{
    //Data
    [SerializeField] public float _curTension;
    [SerializeField] public float _maxTension = 100f;
    public bool HasEvidence = false;

    public float tensionRatio => _curTension / _maxTension;


    public void SyncWithGlobalState()
    {
        if (GlobalStateManager.instance != null)
        {
            _curTension = GlobalStateManager.instance.tension;
        }
    }
    public void ModifyTension(float amount)
    {
        _curTension = Mathf.Clamp(_curTension + amount, 0 , _maxTension);
    }

    public void CollectEvidence()
    {
        HasEvidence = true;
    }

    public bool IsGameOver()
    {
        return _curTension >= _maxTension;
    }
}
