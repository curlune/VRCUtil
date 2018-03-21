using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEditor;
using UnityEngine;

using VRC.Core;

public class VRCMyUtil : EditorWindow {

	[MenuItem("VRChat SDK/Util/Joint Sub Animator Bone")]
	static void Init() {
		var allBones = GetHumanBoneList();
		var avatars = GetAvatarList();
		foreach (var avatar in avatars) {
			var baseAnim = avatar.GetComponent<Animator>();
			if (!baseAnim) {
				continue;
			}
			var childAnims = avatar.GetComponentsInChildren<Animator>();
			foreach (var childAnim in childAnims) {
				JointBones(baseAnim, childAnim, allBones);
			}
		}
	}
	
	static List<VRCSDK2.VRC_AvatarDescriptor> GetAvatarList() {
		// from VRCSDK Editer
		List<VRCSDK2.VRC_AvatarDescriptor> allavatars = VRC.Tools.FindSceneObjectsOfTypeAll<VRCSDK2.VRC_AvatarDescriptor>().ToList();
		// select only the active avatars
		var avatars = allavatars.Where(av => av.gameObject.activeInHierarchy).ToList();
		return avatars;
	}

	static List<HumanBodyBones> GetHumanBoneList() {
		return Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>().ToList();
	}

	static void JointBones(Animator baseAnim, Animator childAnim, List<HumanBodyBones> bodyBones) {
		if (baseAnim == childAnim) {
			return;
		}
		foreach (var bone in bodyBones) {
			var baseTransform = baseAnim.GetBoneTransform(bone);
			var childTransform = childAnim.GetBoneTransform(bone);
			if (!baseTransform || !childTransform) {
				continue;
			}
			var baseObj = baseTransform.gameObject;
			var childObj = childTransform.gameObject;

			// TOOD enable undo
			//Undo.RecordObject(baseObj, "Changed baseObject");
			//EditorUtility.SetDirty(baseObj);

			var baseRig = GetOrCreateComponent<Rigidbody>(baseObj);
			baseRig.useGravity = false;
			baseRig.isKinematic = true;
			baseRig.angularDrag = 0;
			baseRig.mass = 0; // set mimimal value, not zero

			var childRig = GetOrCreateComponent<Rigidbody>(childObj);
			childRig.useGravity = false;
			childRig.angularDrag = 0;
			childRig.mass = 0; // set mimimal value, not zero
			childRig.constraints = RigidbodyConstraints.FreezePosition;

			var joints = childObj.GetComponents<FixedJoint>().ToList();
			if (!joints.Any(joint => joint.connectedBody == baseRig)) {
				var joint = childObj.AddComponent<FixedJoint>();
				joint.connectedBody = baseRig;
			}
		}
	}

	static T GetOrCreateComponent<T>(GameObject o) where T : Component {
		var component = o.GetComponent<T>();
		if (component == null) {
			component = o.AddComponent<T>();
		}
		return component;
	}
}
