using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SizzlePoseSystem : MonoBehaviour
{
    [Tooltip("Used to change whether the visual Sizzle should mimic the procedural animation or hard animation on specific parts of their body")]
    [SerializeField] LerpValues lerpValues;

    [Header("References")]
    [SerializeField] VisualReferenceVariables visualReferences;
    [Tooltip("Used to construct procedural skeleton by referenceing the Visual Skeleton manually constructed")][SerializeField] Transform proceduralRoot;
    [Tooltip("Used to construct hard skeleton by referenceing the Visual Skeleton manually constructed")] [SerializeField] Transform hardRoot;
    //[SerializeField] ProceduralReferenceVariables proceduralReferences;
    //[SerializeField] HardReferenceVariables hardReferences;

    private static int SECTIONCOUNT = 7;

    // Skeletons are split into sections (head, neck, etc) and then the individual bones in them 
    private List<Transform>[] visualSkeleton;
    private List<Transform>[] proceduralSkeleton;
    private List<Transform>[] hardSkeleton;

    // Start is called before the first frame update
    void Start()
    {
        VisualSkeletonSetUp();
        ProceduralAndHardSkeletonsSetUp();
    }

    // Update is called once per frame
    void Update()
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
    }

    private void ProceduralAndHardSkeletonsSetUp()
    {
        proceduralSkeleton = new List<Transform>[SECTIONCOUNT];
        hardSkeleton = new List<Transform>[SECTIONCOUNT];

        // For each section 
        for (int i = 0; i < SECTIONCOUNT; i++)
        {
            // Goes through each bone in section 
            for (int j = 0; j < visualSkeleton[i].Count; j++)
            {
                // Since skeletons follow the exact same naming scheme 
                // we can reference the already completeted visual skeleton 
                // to construct the rest of the skeleton 
                proceduralSkeleton[i].Add( proceduralRoot.Find(visualSkeleton[i][j].name) );
                hardSkeleton[i].Add( hardRoot.Find(hardSkeleton[i][j].name) );
            }
        }
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

    /*private List<Transform> visual = new List<Transform>();

    /// <summary>
    /// Get a list of bones that represent the visual skeleton 
    /// </summary>
    public List<Transform> Visual 
    { 
        get 
        { 
            // Until visual is called space is not filled 
            if(visual.Count == 0)
            {
                visual.AddRange(mouthVisual);
            }

            return visual; 
        } 
    }*/
}

[System.Serializable]
public class ProceduralReferenceVariables
{
    [SerializeField] public List<Transform> mouthProcedural;
    [SerializeField] public List<Transform> neckProcedural;
    [SerializeField] public List<Transform> bodyProcedural ;
    [SerializeField] public List<Transform> legLFProcedural;
    [SerializeField] public List<Transform> legRFProcedural ;
    [SerializeField] public List<Transform> legLBProcedural ;
    [SerializeField] public List<Transform> legRBProcedural ;
}

[System.Serializable]
public class HardReferenceVariables
{
    [SerializeField] public List<Transform> mouthHard;
    [SerializeField] public List<Transform> neckHard;
    [SerializeField] public List<Transform> bodyHard;
    [SerializeField] public List<Transform> legLFHard;
    [SerializeField] public List<Transform> legRFHard;
    [SerializeField] public List<Transform> legLBHard;
    [SerializeField] public List<Transform> legRBHard;
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
