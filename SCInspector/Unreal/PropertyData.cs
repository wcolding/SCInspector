namespace SCInspector.Unreal
{
    public class PropertyData
    {
        public int offset = -1;
        public IntPtr calculated = IntPtr.Zero;
        public PropertyType type;
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
    }

    public class NamePropertyData : IntPropertyData { }

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

            // set?
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
    }

    public class StructPropertyData : PropertyData
    {
        public IntPtr structClassPtr;
        public short size = -1;
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
    }
}