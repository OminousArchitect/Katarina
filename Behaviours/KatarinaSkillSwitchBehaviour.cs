using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;
using TMPro;

namespace Katarina
{
	class KatarinaSkillSwitchBehaviour : MonoBehaviour
	{
		/*
		private CharacterBody body;
		private GameObject switchUIRoot;
		public List<SkillDef> skillDefList = new List<SkillDef>();

		private void OnEnable()
		{
			this.body = base.GetComponent<CharacterBody>();
			On.RoR2.UI.HUD.Update += HUD_Update;
		}

		private void HUD_Update(On.RoR2.UI.HUD.orig_Update orig, HUD self)
		{
			orig.Invoke(self);
			bool flag = self.targetBodyObject && self.targetBodyObject == base.gameObject && self.skillIcons[3];
			if (flag)
			{
				bool flag2 = !this.switchUIRoot &&
				             Util.HasEffectiveAuthority(self.targetBodyObject.GetComponent<NetworkIdentity>());
				if (flag2)
				{
					this.switchUIRoot = new GameObject("SwitchUIContainer");
					this.switchUIRoot.transform.SetParent(self.skillIcons[3].transform.parent);
					this.switchUIRoot.transform.localPosition = new Vector3(50f, 150f, 0f);
					this.switchUIRoot.transform.localScale = Vector3.one;
					for (int i = 0; i < this.skillDefList.Count; i++)
					{
						GameObject gameObject =
							UnityEngine.Object.Instantiate<GameObject>(self.skillIcons[3].gameObject,
								this.switchUIRoot.transform);
						gameObject.transform.localPosition = Vector3.zero;
						bool flag3 = i > 0;
						if (flag3)
						{
							gameObject.transform.localPosition = new Vector3((float) (50 * i), (float) (50 * i), 0f);
							UnityEngine.Object.Destroy(gameObject.transform.Find("SkillBackgroundPanel").gameObject);
							gameObject.transform.SetSiblingIndex(0);
						}
						else
						{
							Transform transform =
								gameObject.transform.Find("SkillBackgroundPanel").Find("SkillKeyText");
							UnityEngine.Object.Destroy(transform.GetComponent<InputBindingDisplayController>());
							transform.GetComponent<TextMeshProUGUI>().text =
								MainPlugin.skillSwitchToggle.Value.ToString();
						}

						gameObject.AddComponent<LayoutGroup>();
						UnityEngine.Object.Destroy(gameObject.GetComponent<SkillIcon>());
						gameObject.transform.Find("IsReadyPanel").gameObject.SetActive(true);
						UnityEngine.Object.Destroy(gameObject.transform.Find("CooldownText").gameObject);
						UnityEngine.Object.Destroy(gameObject.transform.Find("CooldownPanel").gameObject);
						UnityEngine.Object.Destroy(gameObject.transform.Find("Skill4StockRoot").gameObject);
						TooltipProvider component = gameObject.GetComponent<TooltipProvider>();
						component.titleToken = this.skillDefList[i].skillNameToken;
						component.bodyToken = this.skillDefList[i].skillDescriptionToken;
						gameObject.transform.Find("IconPanel").GetComponent<Image>().sprite = this.skillDefList[i].icon;
					}
				}
			}

			bool flag4 = this.body && Util.HasEffectiveAuthority(this.body.networkIdentity);
			if (flag4)
			{
				bool flag5 = Input.GetKeyDown(MainPlugin.skillSwitchToggle.Value) && this.body.skillLocator;
				if (flag5)
				{
					int num = 0;
					for (int j = 0; j < this.skillDefList.Count; j++)
					{
						bool flag6 = this.body.skillLocator.utility.skillDef == this.skillDefList[j];
						if (flag6)
						{
							num = j + 1;
							//Debug.Log("One");
						}
					}

					bool flag7 = num > this.skillDefList.Count - 1;
					if (flag7)
					{
						num = 0;
						//Debug.Log("Two");
					}

					this.body.skillLocator.utility = this.body.skillLocator.FindSkillByDef(this.skillDefList[num]);
				}
			}
		}

		private void OnDisable()
		{
			On.RoR2.UI.HUD.Update -= HUD_Update;
			bool flag = this.switchUIRoot;
			if (flag)
			{
				UnityEngine.Object.Destroy(this.switchUIRoot);
			}
		}
		*/
	}
}
