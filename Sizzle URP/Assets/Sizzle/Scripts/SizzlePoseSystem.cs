using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SizzlePoseSystem : MonoBehaviour
{
    [Tooltip("Tells the system what time of mixing we want to apply to Sizzle:\nMix - Seperate bones can be mixed, only applys rotation thought\nProcedural or Hard - Makes the visual copy position and rotation of either chosen")]
    [SerializeField] MixingModes mixMode = MixingModes.mix;
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

    [Header("Bone Renderers")]
    [SerializeField] BoneRenderer visualBoneRenderer;
    [SerializeField] BoneRenderer procedurualBoneRenderer;
    [SerializeField] BoneRenderer hardBoneRenderer;

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


    public enum MixingModes
    {
        mix, // Only mixes rotations, no positions 
        procedurual,
        hard
    }

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
        float[] lerps = lerpValues.Lerps;
        float sum = 0;

        for (int i = 0; i < SECTIONCOUNT; i++)
        {
            for (int j = 0; j < visualSkeleton[i].Count; j++)
            {
                switch (mixMode)
                {
                    case MixingModes.mix:
                        visualSkeleton[i][j].rotation = Quaternion.Lerp(proceduralSkeleton[i][j].rotation, hardSkeleton[i][j].rotation, lerps[i]);
                        visualSkeleton[i][j].localPosition = Vector3.Lerp(proceduralSkeleton[i][j].localPosition, hardSkeleton[i][j].localPosition, lerps[i]);

                        break;
                    case MixingModes.procedurual:
                        visualSkeleton[i][j].localRotation = proceduralSkeleton[i][j].localRotation;
                        visualSkeleton[i][j].localPosition = proceduralSkeleton[i][j].localPosition;


                        break;
                    case MixingModes.hard:
                        visualSkeleton[i][j].localRotation = hardSkeleton[i][j].localRotation;
                        visualSkeleton[i][j].localPosition = hardSkeleton[i][j].localPosition;

                        break;
                    default:
                        break;
                }
            }
            sum += lerps[i];
        }

        visualBoneRenderer.boneColor = Color.Lerp(procedurualBoneRenderer.boneColor, hardBoneRenderer.boneColor, sum / SECTIONCOUNT);
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

        // Order needs to be reversed because of the direction we find it 
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

    /// <summary>
    /// Get an array of all lerp values during the instance
    /// of being called 
    /// </summary>
    public float[] Lerps
    {
        get
        {
            float[] info = new float[]
            {
                mouthLerp,
                neckLerp,
                bodyLerp,
                legLFLerp,
                legRFLerp,
                legLBLerp,
                legRBLerp
            };

            return info;
        }
    }
}
