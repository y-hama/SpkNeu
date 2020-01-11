using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Source
{
    class ConvolutionForward_Source : SourceCode
    {
        public override string Name
        {
            get { return @"ConvolutionForward_Source"; }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("output_temporary", ObjectType.Array, ElementType.FLOAT);
            AddParameter("output", ObjectType.Array, ElementType.FLOAT);
            AddParameter("kn", ObjectType.Array, ElementType.FLOAT);
            AddParameter("bias", ObjectType.Array, ElementType.FLOAT);

            AddParameter("batsize", ObjectType.Value, ElementType.INT);
            AddParameter("inch", ObjectType.Value, ElementType.INT);
            AddParameter("outch", ObjectType.Value, ElementType.INT);
            AddParameter("iw", ObjectType.Value, ElementType.INT);
            AddParameter("ih", ObjectType.Value, ElementType.INT);
            AddParameter("ow", ObjectType.Value, ElementType.INT);
            AddParameter("oh", ObjectType.Value, ElementType.INT);
            AddParameter("iasize", ObjectType.Value, ElementType.INT);
            AddParameter("oasize", ObjectType.Value, ElementType.INT);
            AddParameter("kasize", ObjectType.Value, ElementType.INT);
            AddParameter("ksize", ObjectType.Value, ElementType.INT);

            AddParameter("activation", ObjectType.Value, ElementType.INT);
            AddParameter("alpha", ObjectType.Value, ElementType.FLOAT);
        }

        protected override void CreateSource()
        {
            GlobalID(3);
            AddMethodBody(@"
                        output[i0 * i1 * oasize + i1 * oasize + i2] = 0;
                        float val = 0;
                        for (int c = 0; c < inch; c++)
                        {
                            int y = (int)(i2 / ow);
                            int x = (int)(i2 - y * ow);
                            int ix, iy, idx, kidx;
                            for (int s = -ksize; s <= ksize; s++)
                            {
                                for (int t = -ksize; t <= ksize; t++)
                                {
                                    ix = x + s; iy = y + t;
                                    if (ix >= 0 && ix < iw && iy >= 0 && iy < ih)
                                    {
                                        idx = i0 * c * iasize + c * iasize + iy * iw + ix;
                                        kidx = c * outch * kasize + i1 * kasize + (t + ksize) * (2 * ksize + 1) + (s + ksize);
                                        val += input[idx] * kn[kidx];
                                    }
                                }
                            }
                            val += bias[i1 * inch + c];
                        }
                        output_temporary[i0 * i1 * oasize + i1 * oasize + i2] = val;
                        if (activation > 0)
                        {
                            output[i0 * i1 * oasize + i1 * oasize + i2] = val < 0 ? val * alpha : val;
                        }
                        else
                        {
                            output[i0 * i1 * oasize + i1 * oasize + i2] = val;
                        }
            ");
        }
    }
}
