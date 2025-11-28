using ProtoBuf;
using System;

using System.Collections.Generic;
using Fantasy;
using Fantasy.Network.Interface;
using Fantasy.Serialize;
#pragma warning disable CS8618

namespace Fantasy
{
	[ProtoContract]
	public partial class C2G_TestMessage : AMessage, IMessage
	{
		public static C2G_TestMessage Create(Scene scene)
		{
			return scene.MessagePoolComponent.Rent<C2G_TestMessage>();
		}
		public override void Dispose()
		{
			Tag = default;
#if FANTASY_NET || FANTASY_UNITY
			GetScene().MessagePoolComponent.Return<C2G_TestMessage>(this);
#endif
		}
		public uint OpCode() { return OuterOpcode.C2G_TestMessage; }
		[ProtoMember(1)]
		public string Tag { get; set; }
	}
	[ProtoContract]
	public partial class C2G_TestRequest : AMessage, IRequest
	{
		public static C2G_TestRequest Create(Scene scene)
		{
			return scene.MessagePoolComponent.Rent<C2G_TestRequest>();
		}
		public override void Dispose()
		{
			Tag = default;
#if FANTASY_NET || FANTASY_UNITY
			GetScene().MessagePoolComponent.Return<C2G_TestRequest>(this);
#endif
		}
		[ProtoIgnore]
		public G2C_TestResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.C2G_TestRequest; }
		[ProtoMember(1)]
		public string Tag { get; set; }
	}
	[ProtoContract]
	public partial class G2C_TestResponse : AMessage, IResponse
	{
		public static G2C_TestResponse Create(Scene scene)
		{
			return scene.MessagePoolComponent.Rent<G2C_TestResponse>();
		}
		public override void Dispose()
		{
			ErrorCode = default;
			Tag = default;
#if FANTASY_NET || FANTASY_UNITY
			GetScene().MessagePoolComponent.Return<G2C_TestResponse>(this);
#endif
		}
		public uint OpCode() { return OuterOpcode.G2C_TestResponse; }
		[ProtoMember(1)]
		public string Tag { get; set; }
		[ProtoMember(2)]
		public uint ErrorCode { get; set; }
	}
	[ProtoContract]
	public partial class C2A_RegisterRequest : AMessage, IRequest
	{
		public static C2A_RegisterRequest Create(Scene scene)
		{
			return scene.MessagePoolComponent.Rent<C2A_RegisterRequest>();
		}
		public override void Dispose()
		{
			userName = default;
			password = default;
#if FANTASY_NET || FANTASY_UNITY
			GetScene().MessagePoolComponent.Return<C2A_RegisterRequest>(this);
#endif
		}
		[ProtoIgnore]
		public A2C_RegisterResponse ResponseType { get; set; }
		public uint OpCode() { return OuterOpcode.C2A_RegisterRequest; }
		[ProtoMember(1)]
		public string userName { get; set; }
		[ProtoMember(2)]
		public string password { get; set; }
	}
	[ProtoContract]
	public partial class A2C_RegisterResponse : AMessage, IResponse
	{
		public static A2C_RegisterResponse Create(Scene scene)
		{
			return scene.MessagePoolComponent.Rent<A2C_RegisterResponse>();
		}
		public override void Dispose()
		{
			ErrorCode = default;
			userName = default;
			registerResult = default;
#if FANTASY_NET || FANTASY_UNITY
			GetScene().MessagePoolComponent.Return<A2C_RegisterResponse>(this);
#endif
		}
		public uint OpCode() { return OuterOpcode.A2C_RegisterResponse; }
		[ProtoMember(1)]
		public string userName { get; set; }
		[ProtoMember(2)]
		public bool registerResult { get; set; }
		[ProtoMember(3)]
		public uint ErrorCode { get; set; }
	}
}

