using UnityEngine;

public class GlobalStateManager : MonoBehaviour
{
    public static GlobalStateManager instance { get; private set; }

    [Header("Core Variables")]
    public float tension = 45;
    public const int SOFT_THRESHOLD = 55;
    public const int HARD_THRESHOLD = 85;

    [Header("Player Actions")]
    public bool hasRecording = false;
    public bool recordingDiscovered = false;
    public bool calledHelp = false;
    public bool teacherOnTheWay = false;
    public bool teacherArrived = false;

    [Header("NPC States")]
    public bool victimSafe = false;
    public bool playerWalkedAway = false;
    public int bystanderSupport = 0; // 0 = no one, 1 = Linh, 2 = more bystanders
    public bool aggressorFocusOnPlayer = false;

    [Header("Reputation & Trust")]
    public float victimTrust = 0f; // -1..1
    public float teacherTrust = 0f; // -1..1
    public float playerReputation = 0f; // -1..1
    public float hungReputation = 0f; // -1..1

    [Header("Social Dynamics")]
    public float rumorLevel = 0f; // 0..1
    public bool socialMediaPosted = false;
    public bool futureThreat = false;

    [Header("Skill & Progress")]
    public int deEscalationSkill = 3; // 1..5
    public string currentNodeId = "S0";
    public string currentEndingId = "";

    //track ending
    public bool lastEndingWasGood = false;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }


    /// Reset state with default values
    public void ResetState()
    {
        ResetState(45);
    }

    /// Reset state with custom initial tension
 public void ResetState(int initialTension)
    {

        tension = Mathf.Clamp(initialTension, 0, 100);
        hasRecording = false;
        recordingDiscovered = false;
        calledHelp = false;
        teacherOnTheWay = false;
        teacherArrived = false;
        victimSafe = false;
        playerWalkedAway = false;
        bystanderSupport = 0;
        aggressorFocusOnPlayer = false;
        
        victimTrust = 0f;
        teacherTrust = 0f;
        playerReputation = 0f;
        hungReputation = 0f;
        
        rumorLevel = 0f;
        socialMediaPosted = false;
        futureThreat = false;
        
        deEscalationSkill = 3;
        currentNodeId = "S0";
        currentEndingId = "";
    }

    /// Set initial tension for new level/scenario
public void SetInitialTension(int value)
    {
        tension = Mathf.Clamp(value, 0, 100);
        Debug.Log($"Initial tension set to: {tension}");
    }

    public void ModifyTension(float amount)
    {
        tension = Mathf.Clamp(tension + amount, 0, 100);

      // Auto trigger if thresholds are reached
        if (tension >= HARD_THRESHOLD)
        {
            Debug.Log("HARD THRESHOLD REACHED!");
  }
        else if (tension >= SOFT_THRESHOLD)
        {
            Debug.Log("SOFT THRESHOLD REACHED!");
        }
    }

    /// Check if reach ending condition
    public bool ShouldTriggerEnding(out string endingId)
    {
        endingId = "";

        // E0: Walk Away
        if(playerWalkedAway)
        {
         endingId = "E0";
            return true;
        }

        // E4: Violence Escalated (HARD_THRESHOLD)
        if(tension >= HARD_THRESHOLD)
        {
            endingId = "E4";
            return true;
        }

        // E6: Social Media Drama
        if(socialMediaPosted)
        {
            endingId = "E6";
            return true;
        }

      return false;
    }
}
