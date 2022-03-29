using UnityEngine;

/// <summary>
/// Represents the paint stand and provides methods to persist and set the orientation and position of the
/// workpiece holder.
/// </summary>
public class PaintStand : MonoBehaviour
{
   [HideInInspector] public bool changedTransform;
   
   private Transform holder1;
   private Transform holder2;
   private Transform holder3;
   private Transform holder4;
   private Transform centralRod;
   private Transform workpiece;

   private void Awake()
   {
      holder1 = transform.FindDeepChild("Holder 1");
      holder2 = transform.FindDeepChild("Holder 2");
      holder3 = transform.FindDeepChild("Holder 3");
      holder4 = transform.FindDeepChild("Holder 4");
      centralRod = transform.FindDeepChild("Central Rod");
      workpiece = transform.parent.GetComponentInChildren<CustomDrawable>().transform;
   }

   public void PersistPaintStand(Frame frame, bool firstFrame)
   {
      if (changedTransform || firstFrame)
      {
         frame.holder1 = new ObjectData(holder1);
         frame.holder2 = new ObjectData(holder2);
         frame.holder3 = new ObjectData(holder3);
         frame.holder4 = new ObjectData(holder4);
         frame.centralRod = new ObjectData(centralRod);
         frame.workpiece = new ObjectData(workpiece);
      }
   }

   public void SetPaintStand(Frame frame, Frame lastFrame)
   {
      if (frame.holder1 != null)
      {
         frame.centralRod.SetTransform(centralRod);
         frame.holder1.SetTransform(holder1);
         frame.holder2.SetTransform(holder2);
         frame.holder3.SetTransform(holder3);
         frame.holder4.SetTransform(holder4);
         frame.workpiece.SetTransform(workpiece);
      }
      
      // needed for reflection tool to simplify jumping
      if (lastFrame != null)
      {
         if (frame.holder1 == null)
            frame.holder1 = lastFrame.holder1;
         if (frame.holder2 == null)
            frame.holder2 = lastFrame.holder2;
         if (frame.holder3 == null)
            frame.holder3 = lastFrame.holder3;
         if (frame.holder4 == null)
            frame.holder4 = lastFrame.holder4;
         if (frame.centralRod == null)
            frame.centralRod = lastFrame.centralRod;
         if (frame.workpiece == null)
            frame.workpiece = lastFrame.workpiece;
      }
   }
}
