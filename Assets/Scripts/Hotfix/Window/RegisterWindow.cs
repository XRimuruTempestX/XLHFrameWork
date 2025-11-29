using XLHFramework.UIFrameWork.Agent; 
using XLHFramework.UIFrameWork.Runtime.Base;
using Cysharp.Threading.Tasks; 
using UnityEngine.UI;
using UnityEngine; 
using TMPro;
using XGC.GameWorld;
using XLHFramework.GCFrameWork.World;
using XLHFramework.UnityDebuger;

namespace UIFrameworlk
{
	public class RegisterWindow : WindowBase
	{
		public RegisterWindowDataWindow dataCompt;

		private string userName;
		private string passWord;
		
		private RegisterLogic _registerLogic;

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
			_registerLogic = GameWorld.GetExitsLogicCtrl<RegisterLogic>();
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
			userName = value;
		}
		public void AddpassWordTMP_InputFieldValueChangedListener(string value)
		{
			
		}
		public void AddpassWordTMP_InputFieldEndEditListener(string value)
		{
			passWord = value;

		}
		public void AddRegisterBtnListener()
		{
			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
			{
				Debuger.LogError("用户名或者密码不能为空！！！！");
				return;
			}

			if (_registerLogic != null)
			{
				_registerLogic.SendRegisterRequest(userName, passWord).Coroutine();
			}
		}
		public void AddCancelBtnListener()
		{
			
		}
	}
}
