using System.Drawing;

namespace SCInspector.Unreal
{
    public class PropertyData
    {
        public int offset = -1;
        public IntPtr calculated = IntPtr.Zero;
        public PropertyType Type;

        public virtual void SetData(IntPtr entry, GameOffsets offsets)
        {
            offset = (int)Memory.ReadUInt32(entry + offsets.PropertyOffset);
        }

        public virtual PropertyData GetCopy() 
        {
            PropertyData copy = new BytePropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class BytePropertyData : PropertyData
    {
        public byte value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return 0;

                return (byte)Memory.ReadUInt8(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt8(calculated, value);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Byte;
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            BytePropertyData copy = new BytePropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class BoolPropertyData : PropertyData
    {
        public uint bitmask;
        public bool value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return false;

                uint bitfield = Memory.ReadUInt32(calculated);
                if ((bitfield & bitmask) != 0)
                    return true;

                return false;
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                uint bitfield = Memory.ReadUInt32(calculated);
                uint newVal = bitfield | bitmask;
                if (!value)
                    newVal ^= bitmask;

                Memory.WriteUInt32(calculated, newVal);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Bool;
            bitmask  = Memory.ReadUInt32(entry + offsets.Bitmask);
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            BoolPropertyData copy = new BoolPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class IntPropertyData : PropertyData
    {
        public int value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return -1;

                return (int)Memory.ReadUInt32(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt32(calculated, (uint)value);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Int;
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            IntPropertyData copy = new IntPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class NamePropertyData : PropertyData
    {
        public int value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return -1;

                return (int)Memory.ReadUInt32(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt32(calculated, (uint)value);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Name;
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            NamePropertyData copy = new NamePropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class StrPropertyData : PropertyData
    {
        public TArray value
        {
            get
            {
                TArray nullTArray = new TArray() { contents = IntPtr.Zero, count = 0, max = 0 };

                if (calculated == IntPtr.Zero)
                    return nullTArray;

                byte[] fstringBuffer = new byte[12];
                Memory.ReadProcessMemory(Memory.ProcessHandle, calculated, fstringBuffer, 12, out Memory.outputPtr);
                if (Memory.outputPtr != IntPtr.Zero)
                {
                    TArray output = new TArray();
                    output.contents = (IntPtr)BitConverter.ToUInt32(fstringBuffer, 0);
                    output.count = BitConverter.ToUInt32(fstringBuffer, 4);
                    output.max = BitConverter.ToUInt32(fstringBuffer, 8);
                    return output;
                }

                return nullTArray;
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.String;
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            StrPropertyData copy = new StrPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class FloatPropertyData : PropertyData
    {
        public float value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return -1;

                return Memory.ReadFloat(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteFloat(calculated, value);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Float;
            base.SetData(entry, offsets);
        }


        public override PropertyData GetCopy()
        {
            FloatPropertyData copy = new FloatPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class ObjectPropertyData : PropertyData
    {
        public IntPtr value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return IntPtr.Zero;

                return (IntPtr)Memory.ReadUInt32(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt32(calculated, (uint)value);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Object;
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            ObjectPropertyData copy = new ObjectPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }

    public class StructPropertyData : PropertyData
    {
        public IntPtr structClassPtr;
        public short size = -1;

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Struct;
            size = (short)Memory.ReadUInt16(entry + offsets.Size); 
            structClassPtr = (IntPtr)Memory.ReadUInt32(entry + offsets.Struct);
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            StructPropertyData copy = new StructPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            copy.structClassPtr = structClassPtr;
            copy.size = size;
            return copy;
        }
    }

    public class ArrayPropertyData : PropertyData
    {
        public TArray value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return new TArray() { contents = IntPtr.Zero, count = 0, max = 0 };

                return Memory.ReadStructure<TArray>(calculated);
            }
        }

        public override void SetData(IntPtr entry, GameOffsets offsets)
        {
            Type = PropertyType.Array;
            base.SetData(entry, offsets);
        }

        public override PropertyData GetCopy()
        {
            ArrayPropertyData copy = new ArrayPropertyData();
            copy.offset = offset;
            copy.Type = Type;
            return copy;
        }
    }
}