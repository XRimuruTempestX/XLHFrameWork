using System.Runtime.CompilerServices;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using System.Collections.Generic;
#pragma warning disable CS8618

namespace Fantasy
{
	public static class NetworkProtocolHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C2G_TestMessage(this Session session, C2G_TestMessage message)
		{
			session.Send(message);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C2G_TestMessage(this Session session, string tag)
		{
			using var message = Fantasy.C2G_TestMessage.Create(session.Scene);
			message.Tag = tag;
			session.Send(message);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_TestResponse> C2G_TestRequest(this Session session, C2G_TestRequest request)
		{
			return (G2C_TestResponse)await session.Call(request);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_TestResponse> C2G_TestRequest(this Session session, string tag)
		{
			using var request = Fantasy.C2G_TestRequest.Create(session.Scene);
			request.Tag = tag;
			return (G2C_TestResponse)await session.Call(request);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<A2C_RegisterResponse> C2A_RegisterRequest(this Session session, C2A_RegisterRequest request)
		{
			return (A2C_RegisterResponse)await session.Call(request);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<A2C_RegisterResponse> C2A_RegisterRequest(this Session session, string userName, string password)
		{
			using var request = Fantasy.C2A_RegisterRequest.Create(session.Scene);
			request.userName = userName;
			request.password = password;
			return (A2C_RegisterResponse)await session.Call(request);
		}

	}
}
