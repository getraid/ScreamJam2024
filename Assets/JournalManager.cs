using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalManager : MonoBehaviour
{
    
    [field: SerializeField] public Animator Animator { get; set; }
    
    
    private static readonly int HasOpened = Animator.StringToHash("HasOpened");
    private static readonly int IsReady = Animator.StringToHash("IsReady");

    private bool testFlipFlop = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            testFlipFlop = !testFlipFlop;
            Animator.SetBool(HasOpened, testFlipFlop);
            Animator.SetBool(IsReady,true);
        }
        
        
    }
}
