using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace HeicToJpegConverter;

public static unsafe class Converter
{
    public static bool ConvertHeicToJpeg(string sourceFilePath, string targetFilePath, IWICImagingFactory* factory)
    {
        IWICBitmapDecoder* decoder = null;
        IWICBitmapFrameDecode* frameDecode = null;
        IWICStream* stream = null;
        IWICBitmapEncoder* encoder = null;
        IWICBitmapFrameEncode* frameEncode = null;
        IPropertyBag2* propertyBag = null;

        try
        {
            fixed (char* pSource = sourceFilePath)
            {
                Guid vendorNull = Guid.Empty;
                int hr = factory->CreateDecoderFromFilename(
                    pSource,
                    &vendorNull,
                    (uint)GENERIC_READ,
                    WICDecodeOptions.WICDecodeMetadataCacheOnDemand,
                    &decoder);
                if (hr < 0) return false;
            }

            int hrDecode = decoder->GetFrame(0, &frameDecode);
            if (hrDecode < 0) return false;

            hrDecode = factory->CreateStream(&stream);
            if (hrDecode < 0) return false;

            fixed (char* pTarget = targetFilePath)
            {
                hrDecode = stream->InitializeFromFilename(pTarget, (uint)GENERIC_WRITE);
                if (hrDecode < 0) return false;
            }

            Guid guidJpeg = new Guid("19e4a5aa-5662-4fc5-a0c0-1758028e1057");
            hrDecode = factory->CreateEncoder(&guidJpeg, null, &encoder);
            if (hrDecode < 0) return false;

            hrDecode = encoder->Initialize((IStream*)stream, WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache);
            if (hrDecode < 0) return false;

            hrDecode = encoder->CreateNewFrame(&frameEncode, &propertyBag);
            if (hrDecode < 0) return false;

            // Set JPEG quality
            PROPBAG2 option = new PROPBAG2();
            fixed (char* optName = "ImageQuality")
            {
                option.pstrName = optName;
                VARIANT varValue = default;
                varValue.vt = (ushort)4; // VT_R4
                *(float*)((byte*)&varValue + 8) = 0.95f;
                hrDecode = propertyBag->Write(1, &option, &varValue);
            }

            hrDecode = frameEncode->Initialize(propertyBag);
            if (hrDecode < 0) return false;

            // Metadata copy
            IWICMetadataBlockReader* blockReader = null;
            Guid uuidBlockReader = typeof(IWICMetadataBlockReader).GUID;
            if (frameDecode->QueryInterface(&uuidBlockReader, (void**)&blockReader) >= 0)
            {
                IWICMetadataBlockWriter* blockWriter = null;
                Guid uuidBlockWriter = typeof(IWICMetadataBlockWriter).GUID;
                if (frameEncode->QueryInterface(&uuidBlockWriter, (void**)&blockWriter) >= 0)
                {
                    blockWriter->InitializeFromBlockReader(blockReader);
                    blockWriter->Release();
                }
                blockReader->Release();
            }

            // Write pixel data
            hrDecode = frameEncode->WriteSource((IWICBitmapSource*)frameDecode, null);
            if (hrDecode < 0) return false;

            hrDecode = frameEncode->Commit();
            if (hrDecode < 0) return false;

            hrDecode = encoder->Commit();
            if (hrDecode < 0) return false;

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (propertyBag != null) propertyBag->Release();
            if (frameEncode != null) frameEncode->Release();
            if (encoder != null) encoder->Release();
            if (stream != null) stream->Release();
            if (frameDecode != null) frameDecode->Release();
            if (decoder != null) decoder->Release();
        }
    }
}
