// Copyright � 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Anki.CozmoAnim
{

using global::System;
using global::FlatBuffers;

public struct AnimClip : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static AnimClip GetRootAsAnimClip(ByteBuffer _bb) { return GetRootAsAnimClip(_bb, new AnimClip()); }
  public static AnimClip GetRootAsAnimClip(ByteBuffer _bb, AnimClip obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public AnimClip __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span(4); }
#else
  public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
  public Keyframes? Keyframes { get { int o = __p.__offset(6); return o != 0 ? (Keyframes?)(new Keyframes()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }

  public static Offset<AnimClip> CreateAnimClip(FlatBufferBuilder builder,
      StringOffset NameOffset = default(StringOffset),
      Offset<Keyframes> keyframesOffset = default(Offset<Keyframes>)) {
    builder.StartObject(2);
    AnimClip.AddKeyframes(builder, keyframesOffset);
    AnimClip.AddName(builder, NameOffset);
    return AnimClip.EndAnimClip(builder);
  }

  public static void StartAnimClip(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddName(FlatBufferBuilder builder, StringOffset NameOffset) { builder.AddOffset(0, NameOffset.Value, 0); }
  public static void AddKeyframes(FlatBufferBuilder builder, Offset<Keyframes> keyframesOffset) { builder.AddOffset(1, keyframesOffset.Value, 0); }
  public static Offset<AnimClip> EndAnimClip(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<AnimClip>(o);
  }
};


}
