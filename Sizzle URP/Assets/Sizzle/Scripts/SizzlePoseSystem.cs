using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SizzlePoseSystem : MonoBehaviour
{
    [Tooltip("Used to change whether the visual Sizzle should mimic the procedural animation or hard animation on specific parts of their body")]
    [SerializeField] LerpValues lerpValues;

    [Header("References")]
    [SerializeField] VisualReferenceVariables visualReferences;
    [Tooltip("Used to construct procedural skeleton by referenceing the Visual Skeleton manually constructed")]
    [SerializeField] Transform proceduralRoot;
    [Tooltip("Used to construct hard skeleton by referenceing the Visual Skeleton manually constructed")] 
    [SerializeField] Transform hardRoot;
    //[SerializeField] ProceduralReferenceVariables proceduralReferences;
    //[SerializeField] HardReferenceVariables hardReferences;

    /// <summary>
    /// The amount of sections that Sizzle is broken up into 
    /// </summary>
    private static int SECTIONCOUNT = 7;

    // Skeletons are split into sections (head, neck, etc) and then the individual bones in them 
    private List<Transform>[] visualSkeleton;
    private List<Transform>[] proceduralSkeleton;
    private List<Transform>[] hardSkeleton;

    // Used to hold a set of directions to each bone by its name 
    private Dictionary<string,List<int>> indexInstructions;

    private void Awake()
    {
        indexInstructions = new Dictionary<string, List<int>>();
    }

    // Start is called before the first frame update
    void Start()
    {
        VisualSkeletonSetUp();
        ProceduralAndHardSkeletonsSetUp();
    }

    // Update is called once per frame
    void Update()
    {
        PoseCopy();
    }


    /// <summary>
    /// By using the lerp values changes the visual skeleton to copy 
    /// the orientation of the procedural or hard skeleton 
    /// </summary>
    private void PoseCopy()
    {

    }


    /// <summary>
    /// Sets up the primiary skeleton used to visualize Sizzle 
    /// </summary>
    private void VisualSkeletonSetUp()
    {
        visualSkeleton = new List<Transform>[SECTIONCOUNT];

        // Adds the skeletons values manually 
        visualSkeleton[0] = visualReferences.mouthVisual;
        visualSkeleton[1] = visualReferences.neckVisual;
        visualSkeleton[2] = visualReferences.bodyVisual;
        visualSkeleton[3] = visualReferences.legLFVisual;
        visualSkeleton[4] = visualReferences.legRFVisual;
        visualSkeleton[5] = visualReferences.legLBVisual;
        visualSkeleton[6] = visualReferences.legRBVisual;

        // Goes through each bone 
        for (int i = 0; i < visualSkeleton.Length; i++)
        {
            for (int j = 0; j < visualSkeleton[i].Count; j++)
            {
                // Gets the instructions to the Visual root 
                // These index instructions can be used for the other two skeletons too 
                indexInstructions.Add(visualSkeleton[i][j].name, CreateInstructionsFromRoot(visualSkeleton[i][j], "Visual"));
            }
        }
    }

    private void ProceduralAndHardSkeletonsSetUp()
    {
        // Creates array of sectionss 
        proceduralSkeleton = new List<Transform>[SECTIONCOUNT];
        hardSkeleton = new List<Transform>[SECTIONCOUNT];

        // For each section 
        for (int i = 0; i < SECTIONCOUNT; i++)
        {
            // Creating a section 
            proceduralSkeleton[i] = new List<Transform>();
            hardSkeleton[i] = new List<Transform>();

            // Goes through each bone in section 
            for (int j = 0; j < visualSkeleton[i].Count; j++)
            {
                // Since skeletons follow the exact same naming scheme 
                // we can reference the already completeted visual skeleton 
                // to construct the rest of the skeleton 
                //proceduralRoot.Find(visualSkeleton[i][j].name) 
                //hardSkeleton[i].Add( hardRoot.Find(hardSkeleton[i][j].name) );

                proceduralSkeleton[i].Add(GetChildFromInstructions(proceduralRoot, indexInstructions[visualSkeleton[i][j].name]));
                hardSkeleton[i].Add(GetChildFromInstructions(hardRoot, indexInstructions[visualSkeleton[i][j].name]));
            }
        }
    }

    /// <summary>
    /// Gets a set of indexes to get to a bone from a root 
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private List<int> CreateInstructionsFromRoot(Transform bone, string rootName)
    {
        List<int> instructions = new List<int>();
        Transform current = bone;
        
        // Repeats until current is the root 
        do
        {
            // Adds current index to group 
            instructions.Add(current.GetSiblingIndex());
            current = current.transform.parent;

        } while (current.name != rootName);

        instructions.Reverse();
        return instructions;
    }

    /// <summary>
    /// By following a set of index instructions get the child that 
    /// is of each subsequent child until the end of the instructions
    /// are reached 
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    private Transform GetChildFromInstructions(Transform root, List<int> instructions)
    {
        Transform current = root;
        for (int i = 0; i < instructions.Count; i++)
        {
            current = current.GetChild(instructions[i]);
        }

        return current;
    }

}



// Grouping data variables for editor 

[System.Serializable]
public class VisualReferenceVariables
{
    [SerializeField] public List<Transform> mouthVisual;
    [SerializeField] public List<Transform> neckVisual;
    [SerializeField] public List<Transform> bodyVisual;
    [SerializeField] public List<Transform> legLFVisual;
    [SerializeField] public List<Transform> legRFVisual;
    [SerializeField] public List<Transform> legLBVisual;
    [SerializeField] public List<Transform> legRBVisual;
}

[System.Serializable]
public class LerpValues
{
    [Range(0, 1)][SerializeField] public float mouthLerp;
    [Range(0, 1)][SerializeField] public float neckLerp;
    [Range(0, 1)][SerializeField] public float bodyLerp;
    [Range(0, 1)][SerializeField] public float legLFLerp;
    [Range(0, 1)][SerializeField] public float legRFLerp;
    [Range(0, 1)][SerializeField] public float legLBLerp;
    [Range(0, 1)][SerializeField] public float legRBLerp;
}
