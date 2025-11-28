using XLHFramework.UIFrameWork.Agent; 
using XLHFramework.UIFrameWork.Runtime.Base;
using Cysharp.Threading.Tasks; 
using UnityEngine.UI;
using UnityEngine; 
using TMPro;

namespace UIFrameworlk
{
	public class RegisterWindow : WindowBase
	{
		public RegisterWindowDataWindow dataCompt;

		public override void OnAwake()
		{
			base.OnAwake();
			dataCompt = gameObject.GetComponent<RegisterWindowDataWindow>();
			dataCompt.InitComponent(this);
		}

		public override async UniTask AnimationBegin()
		{
			await base.AnimationBegin();
		}

		public override async UniTask OnShow()
		{
			await base.OnShow();
		}

		public override async UniTask AnimationEnd()
		{
			await base.AnimationEnd();
		}

		public override async UniTask OnHide()
		{
			await base.OnHide();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		public void AdduserNameTMP_InputFieldValueChangedListener(string value)
		{
			
		}
		public void AdduserNameTMP_InputFieldEndEditListener(string value)
		{
			Debug.Log("userName"+ value);
		}
		public void AddpassWordTMP_InputFieldValueChangedListener(string value)
		{
			
		}
		public void AddpassWordTMP_InputFieldEndEditListener(string value)
		{
			Debug.Log("passWord"+ value);

		}
		public void AddRegisterBtnListener()
		{
			
		}
		public void AddCancelBtnListener()
		{
			
		}
	}
}
