﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TorchSharp.Tensor {

    /// <summary>
    ///   Tensor of type Byte.
    ///   This tensor maps to a Torch variable (see torch/csrc/autograd/variable.h).
    //    Please do no mix Aten Tensors and Torch Tensors.
    /// </summary>
    public struct ByteTensor : ITorchTensor<byte>
    {
        internal IntPtr handle;

        internal ByteTensor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///   Releases the tensor and its associated data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Dispose(IntPtr handle);

        /// <summary>
        ///   Implements the .NET Dispose pattern.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                THS_Dispose(handle);
                handle = IntPtr.Zero;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new_byteScalar(byte scalar);

        public static ByteTensor From(byte scalar)
        {
            return new ByteTensor(THS_new_byteScalar(scalar));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new(IntPtr rawArray, long[] dimensions, int numDimensions, long[] strides, int numStrides, sbyte type);

        public static ByteTensor From(IntPtr rawArray, long[] dimensions)
        {
            var length = dimensions.Length;
            var strides = new long[length];

            strides[0] = 1;
            for (int i = 1; i < length; i++)
            {
                strides[i] = dimensions[i - 1];
            }

            return new ByteTensor(THS_new(rawArray, dimensions, dimensions.Length, strides, strides.Length, (sbyte)ATenScalarMapping.Byte));
        }

        public static ByteTensor From(byte[] rawArray, long[] dimensions)
        {
            unsafe
            {
                fixed (byte* parray = rawArray)
                {
                    return ByteTensor.From((IntPtr)parray, dimensions);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static AtenSharp.ByteTensor.HType THS_getTHTensorUnsafe(IntPtr handle);

        /// <summary>
        ///  Returns the number of dimensions for this tensor
        /// </summary>
        public int Dimensions
        {
            get
            {
                var atenTensor = new AtenSharp.ByteTensor (THS_getTHTensorUnsafe (handle));
                return atenTensor.Dimensions;
            }
        }

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public long NumberOfElements 
        {
            get
            {
                switch (Dimensions)
                {
                    case 0: 
                        return 1;
                    case 1:
                        return (int)Shape[0];
                    default:
                        return (int)Shape.Aggregate((x, y) => x * y);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_data(IntPtr handle);

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public Span<byte> Data
        {
            get
            {               
                if (NumberOfElements > int.MaxValue)
                {
                    throw new ArgumentException("Span only supports up to int.MaxValue elements.");
                }
                unsafe
                {
                    return new System.Span<byte>((void*)THS_data(handle), (int)NumberOfElements);
                }
            }
        }

        public byte Item
        {
            get
            {
                if (NumberOfElements != 1)
                {
                    throw new ArgumentException($"Number of elements in the tensor must be 1");
                }
                return Data[0];
            }
        }

        [DllImport("LibTorchSharp")]
        extern static sbyte THS_Type(IntPtr handle);

        public ATenScalarMapping Type
        {
            get
            {
                return (ATenScalarMapping)THS_Type(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static string THS_deviceType(IntPtr handle);

        public string Device
        {
            get
            {
                return THS_deviceType(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cpu(IntPtr handle);

         public ITorchTensor<byte> Cpu()
        {
            return new ByteTensor(THS_cpu(handle));
        }

         [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cuda(IntPtr handle);

         public ITorchTensor<byte> Cuda()
        {
            return new ByteTensor(THS_cuda(handle));
        }

        /// <summary>
        ///  Retrieves the size of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorDimension(int dim)
        {
            var atenTensor = new AtenSharp.ByteTensor (THS_getTHTensorUnsafe (handle));
            return atenTensor.GetTensorDimension (dim);
        }

        /// <summary>
        /// Returns the tensor shape, this is an array whose size determines the number of dimensions on the tensor, and each element is the size of the dimension
        /// </summary>
        /// <remarks>
        ///     An array of size 0 is used for constants, an array of size 1 is used
        ///     for single-dimension arrays, where the dimension is the value of the
        ///     first element.   And so on.
        /// </remarks>
        public long[] Shape
        {
            get
            {
                var dims = new long[Dimensions];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = GetTensorDimension(i);

                return dims;
            }
        }

        /// <summary>
        ///  Retrieves the stride of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorStride (int dim)
        {
            var atenTensor = new AtenSharp.ByteTensor(THS_getTHTensorUnsafe(handle));
            return atenTensor.GetTensorStride(dim);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_ones(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<byte> Ones(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new ByteTensor (THS_ones ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Byte, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_randn(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<byte> RandomN(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new ByteTensor (THS_randn ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Byte, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Backward(IntPtr handle);

        public void Backward()
        {
            THS_Backward(handle);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Grad(IntPtr handle);

        public ITorchTensor<float> Grad()
        {
            return new FloatTensor(THS_Grad(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_View(IntPtr src, IntPtr shape, int length);

        public ITorchTensor<byte> View(params long[] shape)
        {
            unsafe
            {
                fixed (long* pshape = shape)
                {
                    return new ByteTensor (THS_View (handle, (IntPtr)pshape, shape.Length));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sum(IntPtr src);

        public ITorchTensor<byte> Sum()
        {
            return new ByteTensor(THS_Sum(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Eq(IntPtr src, IntPtr trg);

        public ITorchTensor<U> Eq<U>(ITorchTensor<U> target)
        {
            return THS_Eq(handle, target.Handle).ToTorchTensor<U>();
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sub_(IntPtr src, IntPtr trg, bool is_grad);

        public ITorchTensor<byte> SubInPlace(ITorchTensor<byte> target, bool no_grad = true)
        {
            return new ByteTensor(THS_Sub_(handle, target.Handle, !no_grad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Mul(IntPtr src, byte scalar, bool is_grad);

        public ITorchTensor<byte> Mul(byte scalar, bool noGrad = true)
        {
            return new ByteTensor(THS_Mul(handle, scalar, !noGrad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Argmax(IntPtr src, long dimension, bool keep_dim);

        public ITorchTensor<byte> Argmax(long dimension, bool keepDim = false)
        {
            return new ByteTensor(THS_Argmax(handle, dimension, keepDim));
        }

        /// <summary>
        ///   Get a string representation of the tensor.
        /// </summary>
        public override string ToString()
        {
            var n = Dimensions;
            if (n == 0)
                return "[]";

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < n; i++)
            {
                sb.Append(GetTensorDimension(i));
                if (i + 1 < n)
                    sb.Append("x");
            }
            sb.Append("]");
            sb.Append($", device = {Device}");
            return sb.ToString();
        }
    }
    /// <summary>
    ///   Tensor of type Short.
    ///   This tensor maps to a Torch variable (see torch/csrc/autograd/variable.h).
    //    Please do no mix Aten Tensors and Torch Tensors.
    /// </summary>
    public struct ShortTensor : ITorchTensor<short>
    {
        internal IntPtr handle;

        internal ShortTensor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///   Releases the tensor and its associated data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Dispose(IntPtr handle);

        /// <summary>
        ///   Implements the .NET Dispose pattern.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                THS_Dispose(handle);
                handle = IntPtr.Zero;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new_shortScalar(short scalar);

        public static ShortTensor From(short scalar)
        {
            return new ShortTensor(THS_new_shortScalar(scalar));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new(IntPtr rawArray, long[] dimensions, int numDimensions, long[] strides, int numStrides, sbyte type);

        public static ShortTensor From(IntPtr rawArray, long[] dimensions)
        {
            var length = dimensions.Length;
            var strides = new long[length];

            strides[0] = 1;
            for (int i = 1; i < length; i++)
            {
                strides[i] = dimensions[i - 1];
            }

            return new ShortTensor(THS_new(rawArray, dimensions, dimensions.Length, strides, strides.Length, (sbyte)ATenScalarMapping.Short));
        }

        public static ShortTensor From(short[] rawArray, long[] dimensions)
        {
            unsafe
            {
                fixed (short* parray = rawArray)
                {
                    return ShortTensor.From((IntPtr)parray, dimensions);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static AtenSharp.ShortTensor.HType THS_getTHTensorUnsafe(IntPtr handle);

        /// <summary>
        ///  Returns the number of dimensions for this tensor
        /// </summary>
        public int Dimensions
        {
            get
            {
                var atenTensor = new AtenSharp.ShortTensor (THS_getTHTensorUnsafe (handle));
                return atenTensor.Dimensions;
            }
        }

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public long NumberOfElements 
        {
            get
            {
                switch (Dimensions)
                {
                    case 0: 
                        return 1;
                    case 1:
                        return (int)Shape[0];
                    default:
                        return (int)Shape.Aggregate((x, y) => x * y);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_data(IntPtr handle);

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public Span<short> Data
        {
            get
            {               
                if (NumberOfElements > int.MaxValue)
                {
                    throw new ArgumentException("Span only supports up to int.MaxValue elements.");
                }
                unsafe
                {
                    return new System.Span<short>((void*)THS_data(handle), (int)NumberOfElements);
                }
            }
        }

        public short Item
        {
            get
            {
                if (NumberOfElements != 1)
                {
                    throw new ArgumentException($"Number of elements in the tensor must be 1");
                }
                return Data[0];
            }
        }

        [DllImport("LibTorchSharp")]
        extern static sbyte THS_Type(IntPtr handle);

        public ATenScalarMapping Type
        {
            get
            {
                return (ATenScalarMapping)THS_Type(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static string THS_deviceType(IntPtr handle);

        public string Device
        {
            get
            {
                return THS_deviceType(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cpu(IntPtr handle);

         public ITorchTensor<short> Cpu()
        {
            return new ShortTensor(THS_cpu(handle));
        }

         [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cuda(IntPtr handle);

         public ITorchTensor<short> Cuda()
        {
            return new ShortTensor(THS_cuda(handle));
        }

        /// <summary>
        ///  Retrieves the size of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorDimension(int dim)
        {
            var atenTensor = new AtenSharp.ShortTensor (THS_getTHTensorUnsafe (handle));
            return atenTensor.GetTensorDimension (dim);
        }

        /// <summary>
        /// Returns the tensor shape, this is an array whose size determines the number of dimensions on the tensor, and each element is the size of the dimension
        /// </summary>
        /// <remarks>
        ///     An array of size 0 is used for constants, an array of size 1 is used
        ///     for single-dimension arrays, where the dimension is the value of the
        ///     first element.   And so on.
        /// </remarks>
        public long[] Shape
        {
            get
            {
                var dims = new long[Dimensions];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = GetTensorDimension(i);

                return dims;
            }
        }

        /// <summary>
        ///  Retrieves the stride of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorStride (int dim)
        {
            var atenTensor = new AtenSharp.ShortTensor(THS_getTHTensorUnsafe(handle));
            return atenTensor.GetTensorStride(dim);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_ones(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<short> Ones(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new ShortTensor (THS_ones ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Short, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_randn(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<short> RandomN(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new ShortTensor (THS_randn ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Short, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Backward(IntPtr handle);

        public void Backward()
        {
            THS_Backward(handle);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Grad(IntPtr handle);

        public ITorchTensor<float> Grad()
        {
            return new FloatTensor(THS_Grad(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_View(IntPtr src, IntPtr shape, int length);

        public ITorchTensor<short> View(params long[] shape)
        {
            unsafe
            {
                fixed (long* pshape = shape)
                {
                    return new ShortTensor (THS_View (handle, (IntPtr)pshape, shape.Length));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sum(IntPtr src);

        public ITorchTensor<short> Sum()
        {
            return new ShortTensor(THS_Sum(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Eq(IntPtr src, IntPtr trg);

        public ITorchTensor<U> Eq<U>(ITorchTensor<U> target)
        {
            return THS_Eq(handle, target.Handle).ToTorchTensor<U>();
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sub_(IntPtr src, IntPtr trg, bool is_grad);

        public ITorchTensor<short> SubInPlace(ITorchTensor<short> target, bool no_grad = true)
        {
            return new ShortTensor(THS_Sub_(handle, target.Handle, !no_grad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Mul(IntPtr src, short scalar, bool is_grad);

        public ITorchTensor<short> Mul(short scalar, bool noGrad = true)
        {
            return new ShortTensor(THS_Mul(handle, scalar, !noGrad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Argmax(IntPtr src, long dimension, bool keep_dim);

        public ITorchTensor<short> Argmax(long dimension, bool keepDim = false)
        {
            return new ShortTensor(THS_Argmax(handle, dimension, keepDim));
        }

        /// <summary>
        ///   Get a string representation of the tensor.
        /// </summary>
        public override string ToString()
        {
            var n = Dimensions;
            if (n == 0)
                return "[]";

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < n; i++)
            {
                sb.Append(GetTensorDimension(i));
                if (i + 1 < n)
                    sb.Append("x");
            }
            sb.Append("]");
            sb.Append($", device = {Device}");
            return sb.ToString();
        }
    }
    /// <summary>
    ///   Tensor of type Int.
    ///   This tensor maps to a Torch variable (see torch/csrc/autograd/variable.h).
    //    Please do no mix Aten Tensors and Torch Tensors.
    /// </summary>
    public struct IntTensor : ITorchTensor<int>
    {
        internal IntPtr handle;

        internal IntTensor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///   Releases the tensor and its associated data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Dispose(IntPtr handle);

        /// <summary>
        ///   Implements the .NET Dispose pattern.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                THS_Dispose(handle);
                handle = IntPtr.Zero;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new_intScalar(int scalar);

        public static IntTensor From(int scalar)
        {
            return new IntTensor(THS_new_intScalar(scalar));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new(IntPtr rawArray, long[] dimensions, int numDimensions, long[] strides, int numStrides, sbyte type);

        public static IntTensor From(IntPtr rawArray, long[] dimensions)
        {
            var length = dimensions.Length;
            var strides = new long[length];

            strides[0] = 1;
            for (int i = 1; i < length; i++)
            {
                strides[i] = dimensions[i - 1];
            }

            return new IntTensor(THS_new(rawArray, dimensions, dimensions.Length, strides, strides.Length, (sbyte)ATenScalarMapping.Int));
        }

        public static IntTensor From(int[] rawArray, long[] dimensions)
        {
            unsafe
            {
                fixed (int* parray = rawArray)
                {
                    return IntTensor.From((IntPtr)parray, dimensions);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static AtenSharp.IntTensor.HType THS_getTHTensorUnsafe(IntPtr handle);

        /// <summary>
        ///  Returns the number of dimensions for this tensor
        /// </summary>
        public int Dimensions
        {
            get
            {
                var atenTensor = new AtenSharp.IntTensor (THS_getTHTensorUnsafe (handle));
                return atenTensor.Dimensions;
            }
        }

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public long NumberOfElements 
        {
            get
            {
                switch (Dimensions)
                {
                    case 0: 
                        return 1;
                    case 1:
                        return (int)Shape[0];
                    default:
                        return (int)Shape.Aggregate((x, y) => x * y);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_data(IntPtr handle);

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public Span<int> Data
        {
            get
            {               
                if (NumberOfElements > int.MaxValue)
                {
                    throw new ArgumentException("Span only supports up to int.MaxValue elements.");
                }
                unsafe
                {
                    return new System.Span<int>((void*)THS_data(handle), (int)NumberOfElements);
                }
            }
        }

        public int Item
        {
            get
            {
                if (NumberOfElements != 1)
                {
                    throw new ArgumentException($"Number of elements in the tensor must be 1");
                }
                return Data[0];
            }
        }

        [DllImport("LibTorchSharp")]
        extern static sbyte THS_Type(IntPtr handle);

        public ATenScalarMapping Type
        {
            get
            {
                return (ATenScalarMapping)THS_Type(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static string THS_deviceType(IntPtr handle);

        public string Device
        {
            get
            {
                return THS_deviceType(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cpu(IntPtr handle);

         public ITorchTensor<int> Cpu()
        {
            return new IntTensor(THS_cpu(handle));
        }

         [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cuda(IntPtr handle);

         public ITorchTensor<int> Cuda()
        {
            return new IntTensor(THS_cuda(handle));
        }

        /// <summary>
        ///  Retrieves the size of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorDimension(int dim)
        {
            var atenTensor = new AtenSharp.IntTensor (THS_getTHTensorUnsafe (handle));
            return atenTensor.GetTensorDimension (dim);
        }

        /// <summary>
        /// Returns the tensor shape, this is an array whose size determines the number of dimensions on the tensor, and each element is the size of the dimension
        /// </summary>
        /// <remarks>
        ///     An array of size 0 is used for constants, an array of size 1 is used
        ///     for single-dimension arrays, where the dimension is the value of the
        ///     first element.   And so on.
        /// </remarks>
        public long[] Shape
        {
            get
            {
                var dims = new long[Dimensions];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = GetTensorDimension(i);

                return dims;
            }
        }

        /// <summary>
        ///  Retrieves the stride of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorStride (int dim)
        {
            var atenTensor = new AtenSharp.IntTensor(THS_getTHTensorUnsafe(handle));
            return atenTensor.GetTensorStride(dim);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_ones(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<int> Ones(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new IntTensor (THS_ones ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Int, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_randn(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<int> RandomN(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new IntTensor (THS_randn ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Int, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Backward(IntPtr handle);

        public void Backward()
        {
            THS_Backward(handle);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Grad(IntPtr handle);

        public ITorchTensor<float> Grad()
        {
            return new FloatTensor(THS_Grad(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_View(IntPtr src, IntPtr shape, int length);

        public ITorchTensor<int> View(params long[] shape)
        {
            unsafe
            {
                fixed (long* pshape = shape)
                {
                    return new IntTensor (THS_View (handle, (IntPtr)pshape, shape.Length));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sum(IntPtr src);

        public ITorchTensor<int> Sum()
        {
            return new IntTensor(THS_Sum(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Eq(IntPtr src, IntPtr trg);

        public ITorchTensor<U> Eq<U>(ITorchTensor<U> target)
        {
            return THS_Eq(handle, target.Handle).ToTorchTensor<U>();
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sub_(IntPtr src, IntPtr trg, bool is_grad);

        public ITorchTensor<int> SubInPlace(ITorchTensor<int> target, bool no_grad = true)
        {
            return new IntTensor(THS_Sub_(handle, target.Handle, !no_grad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Mul(IntPtr src, int scalar, bool is_grad);

        public ITorchTensor<int> Mul(int scalar, bool noGrad = true)
        {
            return new IntTensor(THS_Mul(handle, scalar, !noGrad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Argmax(IntPtr src, long dimension, bool keep_dim);

        public ITorchTensor<int> Argmax(long dimension, bool keepDim = false)
        {
            return new IntTensor(THS_Argmax(handle, dimension, keepDim));
        }

        /// <summary>
        ///   Get a string representation of the tensor.
        /// </summary>
        public override string ToString()
        {
            var n = Dimensions;
            if (n == 0)
                return "[]";

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < n; i++)
            {
                sb.Append(GetTensorDimension(i));
                if (i + 1 < n)
                    sb.Append("x");
            }
            sb.Append("]");
            sb.Append($", device = {Device}");
            return sb.ToString();
        }
    }
    /// <summary>
    ///   Tensor of type Long.
    ///   This tensor maps to a Torch variable (see torch/csrc/autograd/variable.h).
    //    Please do no mix Aten Tensors and Torch Tensors.
    /// </summary>
    public struct LongTensor : ITorchTensor<long>
    {
        internal IntPtr handle;

        internal LongTensor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///   Releases the tensor and its associated data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Dispose(IntPtr handle);

        /// <summary>
        ///   Implements the .NET Dispose pattern.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                THS_Dispose(handle);
                handle = IntPtr.Zero;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new_longScalar(long scalar);

        public static LongTensor From(long scalar)
        {
            return new LongTensor(THS_new_longScalar(scalar));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new(IntPtr rawArray, long[] dimensions, int numDimensions, long[] strides, int numStrides, sbyte type);

        public static LongTensor From(IntPtr rawArray, long[] dimensions)
        {
            var length = dimensions.Length;
            var strides = new long[length];

            strides[0] = 1;
            for (int i = 1; i < length; i++)
            {
                strides[i] = dimensions[i - 1];
            }

            return new LongTensor(THS_new(rawArray, dimensions, dimensions.Length, strides, strides.Length, (sbyte)ATenScalarMapping.Long));
        }

        public static LongTensor From(long[] rawArray, long[] dimensions)
        {
            unsafe
            {
                fixed (long* parray = rawArray)
                {
                    return LongTensor.From((IntPtr)parray, dimensions);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static AtenSharp.LongTensor.HType THS_getTHTensorUnsafe(IntPtr handle);

        /// <summary>
        ///  Returns the number of dimensions for this tensor
        /// </summary>
        public int Dimensions
        {
            get
            {
                var atenTensor = new AtenSharp.LongTensor (THS_getTHTensorUnsafe (handle));
                return atenTensor.Dimensions;
            }
        }

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public long NumberOfElements 
        {
            get
            {
                switch (Dimensions)
                {
                    case 0: 
                        return 1;
                    case 1:
                        return (int)Shape[0];
                    default:
                        return (int)Shape.Aggregate((x, y) => x * y);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_data(IntPtr handle);

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public Span<long> Data
        {
            get
            {               
                if (NumberOfElements > int.MaxValue)
                {
                    throw new ArgumentException("Span only supports up to int.MaxValue elements.");
                }
                unsafe
                {
                    return new System.Span<long>((void*)THS_data(handle), (int)NumberOfElements);
                }
            }
        }

        public long Item
        {
            get
            {
                if (NumberOfElements != 1)
                {
                    throw new ArgumentException($"Number of elements in the tensor must be 1");
                }
                return Data[0];
            }
        }

        [DllImport("LibTorchSharp")]
        extern static sbyte THS_Type(IntPtr handle);

        public ATenScalarMapping Type
        {
            get
            {
                return (ATenScalarMapping)THS_Type(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static string THS_deviceType(IntPtr handle);

        public string Device
        {
            get
            {
                return THS_deviceType(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cpu(IntPtr handle);

         public ITorchTensor<long> Cpu()
        {
            return new LongTensor(THS_cpu(handle));
        }

         [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cuda(IntPtr handle);

         public ITorchTensor<long> Cuda()
        {
            return new LongTensor(THS_cuda(handle));
        }

        /// <summary>
        ///  Retrieves the size of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorDimension(int dim)
        {
            var atenTensor = new AtenSharp.LongTensor (THS_getTHTensorUnsafe (handle));
            return atenTensor.GetTensorDimension (dim);
        }

        /// <summary>
        /// Returns the tensor shape, this is an array whose size determines the number of dimensions on the tensor, and each element is the size of the dimension
        /// </summary>
        /// <remarks>
        ///     An array of size 0 is used for constants, an array of size 1 is used
        ///     for single-dimension arrays, where the dimension is the value of the
        ///     first element.   And so on.
        /// </remarks>
        public long[] Shape
        {
            get
            {
                var dims = new long[Dimensions];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = GetTensorDimension(i);

                return dims;
            }
        }

        /// <summary>
        ///  Retrieves the stride of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorStride (int dim)
        {
            var atenTensor = new AtenSharp.LongTensor(THS_getTHTensorUnsafe(handle));
            return atenTensor.GetTensorStride(dim);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_ones(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<long> Ones(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new LongTensor (THS_ones ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Long, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_randn(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<long> RandomN(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new LongTensor (THS_randn ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Long, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Backward(IntPtr handle);

        public void Backward()
        {
            THS_Backward(handle);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Grad(IntPtr handle);

        public ITorchTensor<float> Grad()
        {
            return new FloatTensor(THS_Grad(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_View(IntPtr src, IntPtr shape, int length);

        public ITorchTensor<long> View(params long[] shape)
        {
            unsafe
            {
                fixed (long* pshape = shape)
                {
                    return new LongTensor (THS_View (handle, (IntPtr)pshape, shape.Length));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sum(IntPtr src);

        public ITorchTensor<long> Sum()
        {
            return new LongTensor(THS_Sum(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Eq(IntPtr src, IntPtr trg);

        public ITorchTensor<U> Eq<U>(ITorchTensor<U> target)
        {
            return THS_Eq(handle, target.Handle).ToTorchTensor<U>();
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sub_(IntPtr src, IntPtr trg, bool is_grad);

        public ITorchTensor<long> SubInPlace(ITorchTensor<long> target, bool no_grad = true)
        {
            return new LongTensor(THS_Sub_(handle, target.Handle, !no_grad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Mul(IntPtr src, long scalar, bool is_grad);

        public ITorchTensor<long> Mul(long scalar, bool noGrad = true)
        {
            return new LongTensor(THS_Mul(handle, scalar, !noGrad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Argmax(IntPtr src, long dimension, bool keep_dim);

        public ITorchTensor<long> Argmax(long dimension, bool keepDim = false)
        {
            return new LongTensor(THS_Argmax(handle, dimension, keepDim));
        }

        /// <summary>
        ///   Get a string representation of the tensor.
        /// </summary>
        public override string ToString()
        {
            var n = Dimensions;
            if (n == 0)
                return "[]";

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < n; i++)
            {
                sb.Append(GetTensorDimension(i));
                if (i + 1 < n)
                    sb.Append("x");
            }
            sb.Append("]");
            sb.Append($", device = {Device}");
            return sb.ToString();
        }
    }
    /// <summary>
    ///   Tensor of type Double.
    ///   This tensor maps to a Torch variable (see torch/csrc/autograd/variable.h).
    //    Please do no mix Aten Tensors and Torch Tensors.
    /// </summary>
    public struct DoubleTensor : ITorchTensor<double>
    {
        internal IntPtr handle;

        internal DoubleTensor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///   Releases the tensor and its associated data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Dispose(IntPtr handle);

        /// <summary>
        ///   Implements the .NET Dispose pattern.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                THS_Dispose(handle);
                handle = IntPtr.Zero;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new_doubleScalar(double scalar);

        public static DoubleTensor From(double scalar)
        {
            return new DoubleTensor(THS_new_doubleScalar(scalar));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new(IntPtr rawArray, long[] dimensions, int numDimensions, long[] strides, int numStrides, sbyte type);

        public static DoubleTensor From(IntPtr rawArray, long[] dimensions)
        {
            var length = dimensions.Length;
            var strides = new long[length];

            strides[0] = 1;
            for (int i = 1; i < length; i++)
            {
                strides[i] = dimensions[i - 1];
            }

            return new DoubleTensor(THS_new(rawArray, dimensions, dimensions.Length, strides, strides.Length, (sbyte)ATenScalarMapping.Double));
        }

        public static DoubleTensor From(double[] rawArray, long[] dimensions)
        {
            unsafe
            {
                fixed (double* parray = rawArray)
                {
                    return DoubleTensor.From((IntPtr)parray, dimensions);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static AtenSharp.DoubleTensor.HType THS_getTHTensorUnsafe(IntPtr handle);

        /// <summary>
        ///  Returns the number of dimensions for this tensor
        /// </summary>
        public int Dimensions
        {
            get
            {
                var atenTensor = new AtenSharp.DoubleTensor (THS_getTHTensorUnsafe (handle));
                return atenTensor.Dimensions;
            }
        }

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public long NumberOfElements 
        {
            get
            {
                switch (Dimensions)
                {
                    case 0: 
                        return 1;
                    case 1:
                        return (int)Shape[0];
                    default:
                        return (int)Shape.Aggregate((x, y) => x * y);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_data(IntPtr handle);

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public Span<double> Data
        {
            get
            {               
                if (NumberOfElements > int.MaxValue)
                {
                    throw new ArgumentException("Span only supports up to int.MaxValue elements.");
                }
                unsafe
                {
                    return new System.Span<double>((void*)THS_data(handle), (int)NumberOfElements);
                }
            }
        }

        public double Item
        {
            get
            {
                if (NumberOfElements != 1)
                {
                    throw new ArgumentException($"Number of elements in the tensor must be 1");
                }
                return Data[0];
            }
        }

        [DllImport("LibTorchSharp")]
        extern static sbyte THS_Type(IntPtr handle);

        public ATenScalarMapping Type
        {
            get
            {
                return (ATenScalarMapping)THS_Type(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static string THS_deviceType(IntPtr handle);

        public string Device
        {
            get
            {
                return THS_deviceType(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cpu(IntPtr handle);

         public ITorchTensor<double> Cpu()
        {
            return new DoubleTensor(THS_cpu(handle));
        }

         [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cuda(IntPtr handle);

         public ITorchTensor<double> Cuda()
        {
            return new DoubleTensor(THS_cuda(handle));
        }

        /// <summary>
        ///  Retrieves the size of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorDimension(int dim)
        {
            var atenTensor = new AtenSharp.DoubleTensor (THS_getTHTensorUnsafe (handle));
            return atenTensor.GetTensorDimension (dim);
        }

        /// <summary>
        /// Returns the tensor shape, this is an array whose size determines the number of dimensions on the tensor, and each element is the size of the dimension
        /// </summary>
        /// <remarks>
        ///     An array of size 0 is used for constants, an array of size 1 is used
        ///     for single-dimension arrays, where the dimension is the value of the
        ///     first element.   And so on.
        /// </remarks>
        public long[] Shape
        {
            get
            {
                var dims = new long[Dimensions];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = GetTensorDimension(i);

                return dims;
            }
        }

        /// <summary>
        ///  Retrieves the stride of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorStride (int dim)
        {
            var atenTensor = new AtenSharp.DoubleTensor(THS_getTHTensorUnsafe(handle));
            return atenTensor.GetTensorStride(dim);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_ones(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<double> Ones(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new DoubleTensor (THS_ones ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Double, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_randn(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<double> RandomN(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new DoubleTensor (THS_randn ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Double, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Backward(IntPtr handle);

        public void Backward()
        {
            THS_Backward(handle);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Grad(IntPtr handle);

        public ITorchTensor<float> Grad()
        {
            return new FloatTensor(THS_Grad(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_View(IntPtr src, IntPtr shape, int length);

        public ITorchTensor<double> View(params long[] shape)
        {
            unsafe
            {
                fixed (long* pshape = shape)
                {
                    return new DoubleTensor (THS_View (handle, (IntPtr)pshape, shape.Length));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sum(IntPtr src);

        public ITorchTensor<double> Sum()
        {
            return new DoubleTensor(THS_Sum(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Eq(IntPtr src, IntPtr trg);

        public ITorchTensor<U> Eq<U>(ITorchTensor<U> target)
        {
            return THS_Eq(handle, target.Handle).ToTorchTensor<U>();
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sub_(IntPtr src, IntPtr trg, bool is_grad);

        public ITorchTensor<double> SubInPlace(ITorchTensor<double> target, bool no_grad = true)
        {
            return new DoubleTensor(THS_Sub_(handle, target.Handle, !no_grad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Mul(IntPtr src, double scalar, bool is_grad);

        public ITorchTensor<double> Mul(double scalar, bool noGrad = true)
        {
            return new DoubleTensor(THS_Mul(handle, scalar, !noGrad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Argmax(IntPtr src, long dimension, bool keep_dim);

        public ITorchTensor<double> Argmax(long dimension, bool keepDim = false)
        {
            return new DoubleTensor(THS_Argmax(handle, dimension, keepDim));
        }

        /// <summary>
        ///   Get a string representation of the tensor.
        /// </summary>
        public override string ToString()
        {
            var n = Dimensions;
            if (n == 0)
                return "[]";

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < n; i++)
            {
                sb.Append(GetTensorDimension(i));
                if (i + 1 < n)
                    sb.Append("x");
            }
            sb.Append("]");
            sb.Append($", device = {Device}");
            return sb.ToString();
        }
    }
    /// <summary>
    ///   Tensor of type Float.
    ///   This tensor maps to a Torch variable (see torch/csrc/autograd/variable.h).
    //    Please do no mix Aten Tensors and Torch Tensors.
    /// </summary>
    public struct FloatTensor : ITorchTensor<float>
    {
        internal IntPtr handle;

        internal FloatTensor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///   Releases the tensor and its associated data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Dispose(IntPtr handle);

        /// <summary>
        ///   Implements the .NET Dispose pattern.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                THS_Dispose(handle);
                handle = IntPtr.Zero;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new_floatScalar(float scalar);

        public static FloatTensor From(float scalar)
        {
            return new FloatTensor(THS_new_floatScalar(scalar));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_new(IntPtr rawArray, long[] dimensions, int numDimensions, long[] strides, int numStrides, sbyte type);

        public static FloatTensor From(IntPtr rawArray, long[] dimensions)
        {
            var length = dimensions.Length;
            var strides = new long[length];

            strides[0] = 1;
            for (int i = 1; i < length; i++)
            {
                strides[i] = dimensions[i - 1];
            }

            return new FloatTensor(THS_new(rawArray, dimensions, dimensions.Length, strides, strides.Length, (sbyte)ATenScalarMapping.Float));
        }

        public static FloatTensor From(float[] rawArray, long[] dimensions)
        {
            unsafe
            {
                fixed (float* parray = rawArray)
                {
                    return FloatTensor.From((IntPtr)parray, dimensions);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static AtenSharp.FloatTensor.HType THS_getTHTensorUnsafe(IntPtr handle);

        /// <summary>
        ///  Returns the number of dimensions for this tensor
        /// </summary>
        public int Dimensions
        {
            get
            {
                var atenTensor = new AtenSharp.FloatTensor (THS_getTHTensorUnsafe (handle));
                return atenTensor.Dimensions;
            }
        }

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public long NumberOfElements 
        {
            get
            {
                switch (Dimensions)
                {
                    case 0: 
                        return 1;
                    case 1:
                        return (int)Shape[0];
                    default:
                        return (int)Shape.Aggregate((x, y) => x * y);
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_data(IntPtr handle);

        /// <summary>
        ///  Returns a pointer to the unmanaged data managed by this tensor.
        /// </summary>
        public Span<float> Data
        {
            get
            {               
                if (NumberOfElements > int.MaxValue)
                {
                    throw new ArgumentException("Span only supports up to int.MaxValue elements.");
                }
                unsafe
                {
                    return new System.Span<float>((void*)THS_data(handle), (int)NumberOfElements);
                }
            }
        }

        public float Item
        {
            get
            {
                if (NumberOfElements != 1)
                {
                    throw new ArgumentException($"Number of elements in the tensor must be 1");
                }
                return Data[0];
            }
        }

        [DllImport("LibTorchSharp")]
        extern static sbyte THS_Type(IntPtr handle);

        public ATenScalarMapping Type
        {
            get
            {
                return (ATenScalarMapping)THS_Type(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static string THS_deviceType(IntPtr handle);

        public string Device
        {
            get
            {
                return THS_deviceType(handle);
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cpu(IntPtr handle);

         public ITorchTensor<float> Cpu()
        {
            return new FloatTensor(THS_cpu(handle));
        }

         [DllImport("LibTorchSharp")]
        extern static IntPtr THS_cuda(IntPtr handle);

         public ITorchTensor<float> Cuda()
        {
            return new FloatTensor(THS_cuda(handle));
        }

        /// <summary>
        ///  Retrieves the size of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorDimension(int dim)
        {
            var atenTensor = new AtenSharp.FloatTensor (THS_getTHTensorUnsafe (handle));
            return atenTensor.GetTensorDimension (dim);
        }

        /// <summary>
        /// Returns the tensor shape, this is an array whose size determines the number of dimensions on the tensor, and each element is the size of the dimension
        /// </summary>
        /// <remarks>
        ///     An array of size 0 is used for constants, an array of size 1 is used
        ///     for single-dimension arrays, where the dimension is the value of the
        ///     first element.   And so on.
        /// </remarks>
        public long[] Shape
        {
            get
            {
                var dims = new long[Dimensions];
                for (int i = 0; i < dims.Length; i++)
                    dims[i] = GetTensorDimension(i);

                return dims;
            }
        }

        /// <summary>
        ///  Retrieves the stride of the specified dimension in the tensor.
        /// </summary>
        public long GetTensorStride (int dim)
        {
            var atenTensor = new AtenSharp.FloatTensor(THS_getTHTensorUnsafe(handle));
            return atenTensor.GetTensorStride(dim);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_ones(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<float> Ones(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new FloatTensor (THS_ones ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Float, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_randn(IntPtr psizes, int scalarType, int length, string device, bool requireGrad);

        /// <summary>
        ///  Create a new tensor filled with ones
        /// </summary>
        static public ITorchTensor<float> RandomN(long[] size, string device = "cpu", bool requiresGrad = false)
        {
            unsafe
            {
                fixed (long* psizes = size)
                {
                    return new FloatTensor (THS_randn ((IntPtr)psizes, size.Length, (sbyte)ATenScalarMapping.Float, device, requiresGrad));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static void THS_Backward(IntPtr handle);

        public void Backward()
        {
            THS_Backward(handle);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Grad(IntPtr handle);

        public ITorchTensor<float> Grad()
        {
            return new FloatTensor(THS_Grad(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_View(IntPtr src, IntPtr shape, int length);

        public ITorchTensor<float> View(params long[] shape)
        {
            unsafe
            {
                fixed (long* pshape = shape)
                {
                    return new FloatTensor (THS_View (handle, (IntPtr)pshape, shape.Length));
                }
            }
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sum(IntPtr src);

        public ITorchTensor<float> Sum()
        {
            return new FloatTensor(THS_Sum(handle));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Eq(IntPtr src, IntPtr trg);

        public ITorchTensor<U> Eq<U>(ITorchTensor<U> target)
        {
            return THS_Eq(handle, target.Handle).ToTorchTensor<U>();
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Sub_(IntPtr src, IntPtr trg, bool is_grad);

        public ITorchTensor<float> SubInPlace(ITorchTensor<float> target, bool no_grad = true)
        {
            return new FloatTensor(THS_Sub_(handle, target.Handle, !no_grad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Mul(IntPtr src, float scalar, bool is_grad);

        public ITorchTensor<float> Mul(float scalar, bool noGrad = true)
        {
            return new FloatTensor(THS_Mul(handle, scalar, !noGrad));
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THS_Argmax(IntPtr src, long dimension, bool keep_dim);

        public ITorchTensor<float> Argmax(long dimension, bool keepDim = false)
        {
            return new FloatTensor(THS_Argmax(handle, dimension, keepDim));
        }

        /// <summary>
        ///   Get a string representation of the tensor.
        /// </summary>
        public override string ToString()
        {
            var n = Dimensions;
            if (n == 0)
                return "[]";

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < n; i++)
            {
                sb.Append(GetTensorDimension(i));
                if (i + 1 < n)
                    sb.Append("x");
            }
            sb.Append("]");
            sb.Append($", device = {Device}");
            return sb.ToString();
        }
    }
    
    public enum ATenScalarMapping : sbyte
    {
        Byte = 0,
        Short = 2,
        Int = 3,
        Long = 4,
        Float = 6,
        Double = 7
    }

    public static class TensorExtensionMethods
    {
        internal static ITorchTensor<T> ToTorchTensor<T>(this IntPtr rawTensor)
        {
            switch (true)
            {
                case bool _ when typeof(T) == typeof(byte):
                {
                    return new ByteTensor(rawTensor) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(short):
                {
                    return new ShortTensor(rawTensor) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(int):
                {
                    return new IntTensor(rawTensor) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(long):
                {
                    return new LongTensor(rawTensor) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(double):
                {
                    return new DoubleTensor(rawTensor) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(float):
                {
                    return new FloatTensor(rawTensor) as ITorchTensor<T>;
                }
                default: throw new NotImplementedException($"Creating tensor of type {typeof(T)} is not supported.");
            }
        }

        public static ITorchTensor<T> ToTorchTensor<T>(this T[] rawArray, long[] dimensions)
        {
            switch (true)
            {
                case bool _ when typeof(T) == typeof(byte):
                {
                    return ByteTensor.From(rawArray as byte[], dimensions) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(short):
                {
                    return ShortTensor.From(rawArray as short[], dimensions) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(int):
                {
                    return IntTensor.From(rawArray as int[], dimensions) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(long):
                {
                    return LongTensor.From(rawArray as long[], dimensions) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(double):
                {
                    return DoubleTensor.From(rawArray as double[], dimensions) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(float):
                {
                    return FloatTensor.From(rawArray as float[], dimensions) as ITorchTensor<T>;
                }
                default: throw new NotImplementedException($"Creating tensor of type {typeof(T)} is not supported.");
            }
        }

        public static ITorchTensor<T> ToTorchTensor<T>(this T scalar)
        {
            switch (true)
            {
                case bool _ when typeof(T) == typeof(byte):
                {
                    return ByteTensor.From((byte)(object)scalar) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(short):
                {
                    return ShortTensor.From((short)(object)scalar) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(int):
                {
                    return IntTensor.From((int)(object)scalar) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(long):
                {
                    return LongTensor.From((long)(object)scalar) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(double):
                {
                    return DoubleTensor.From((double)(object)scalar) as ITorchTensor<T>;
                }
                case bool _ when typeof(T) == typeof(float):
                {
                    return FloatTensor.From((float)(object)scalar) as ITorchTensor<T>;
                }
                default: throw new NotImplementedException($"Creating tensor of type {typeof(T)} is not supported.");
            }
        }
    }
}
