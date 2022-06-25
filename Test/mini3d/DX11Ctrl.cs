using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security;
#pragma warning disable CS0649, CS8618, CS8600, CS8602, CS8604

namespace Test
{
  [DesignerCategory("Code")]
  public unsafe abstract partial class DX11Ctrl : UserControl
  {
    protected static void Initialize(long drv)
    {
      if (StackPtr != null) return;
      StackPtr = BasePtr = (byte*)VirtualAlloc(null, maxstack, 0x00001000 | 0x00002000, 0x04); //MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE   
      cb1 = (cbPerObject*)StackPtr; StackPtr += sizeof(cbPerObject) << 1;
      cb2 = (cbPerFrame*)StackPtr; StackPtr += sizeof(cbPerFrame) << 1;
      cb3 = (cbPsPerObject*)StackPtr; StackPtr += sizeof(cbPsPerObject) << 1;
      cb4 = (cbPerTexture*)StackPtr; StackPtr += sizeof(cbPerTexture) << 1;
      currentvp = (VIEWPORT*)StackPtr; StackPtr += sizeof(VIEWPORT);
      memset(BasePtr, 0, (nint)(StackPtr - BasePtr)); //376 + 128 
      CreateDriver((uint)((drvsettings = drv) & 0xffffffff));
      Application.ApplicationExit += reset;
    }
    static void CreateDriver(uint id)
    {
      var adapter = (IAdapter)null;
      if (id != 0)
      {
        object unk; CreateDXGIFactory(typeof(IFactory).GUID, out unk); var factory = (IFactory)unk;
        for (int i = 0; factory.EnumAdapters(i, out adapter) == 0 && adapter.Desc.DeviceId != id; i++) ;
      }
      Vector4 tmp; var levels = (FEATURE_LEVEL*)&tmp;
      levels[0] = FEATURE_LEVEL._11_0;
      levels[1] = FEATURE_LEVEL._10_1;
      levels[2] = FEATURE_LEVEL._10_0;
      int hr = D3D11CreateDevice(adapter, adapter != null ? D3D_DRIVER_TYPE.Unknown : D3D_DRIVER_TYPE.Hardware, null, //CREATE_DEVICE_FLAG.Debug |
        CREATE_DEVICE_FLAG.SingleThreaded | CREATE_DEVICE_FLAG.BGRA_Support, levels, 3, SDK_VERSION.Current, out device, null, out context);
      if (hr < 0) { if (id == 0) throw new Exception("D3D11CreateDevice failed!"); CreateDriver(0); return; }
      cbperobject = CreateConstBuffer(sizeof(cbPerObject));
      cbperframe = CreateConstBuffer(sizeof(cbPerFrame));
      cbpsperobject = CreateConstBuffer(sizeof(cbPsPerObject));
      cbpertexture = CreateConstBuffer(sizeof(cbPerTexture)); void* t;
      t = cbperframe; context.VSSetConstantBuffers(0, 1, &t);
      t = cbperframe; context.GSSetConstantBuffers(0, 1, &t);
      t = cbperobject; context.VSSetConstantBuffers(1, 1, &t);
      t = cbpsperobject; context.PSSetConstantBuffers(1, 1, &t);
      t = cbpertexture; context.VSSetConstantBuffers(2, 1, &t);
    }
    static void reset(object? sender, EventArgs? e)
    {
      curtex = curib = curvb = string.Empty;
      release(vertexshader); release(geoshader); release(pixelshader); release(depthstencil); release(blend); release(rasterizer); release(sampler);
      release(ref vertexlayout); release(ref ringbuffer); release(ref cbperobject); release(ref cbpertexture); release(ref cbperframe); release(ref cbpsperobject);
      release(ref rtvtex); release(ref dsvtex); release(ref rtvcpu); release(ref dsvcpu); release(ref rtv1); release(ref dsv1);
      if (context != null) { Marshal.ReleaseComObject(context); context = null; }
      if (device != null) { var c = Marshal.ReleaseComObject(device); device = null; }
      if (wdc != null) { ReleaseDC(null, wdc); wdc = null; }
    }

    static long drvsettings;
    const int maxstack = 10_000_000;
    static byte* BasePtr; static byte* StackPtr;
    static void* wdc;

    protected uint BkColor { get; set; }
    protected abstract void OnRender(DC dc);
    protected abstract int OnMouse(int id, PC dc);
    public Action? Animations;
    public Action? Invaliated;
    protected long TimerTicks;

    VIEWPORT viewport; bool inval;
    ISwapChain? swapchain; void* rtv, dsv; //IRenderTargetView, IDepthStencilView

    void sizebuffers()
    {
      var cs = this.ClientSize;
      viewport.Width = cs.Width = Math.Max(cs.Width, 1);
      viewport.Height = cs.Height = Math.Max(cs.Height, 1); viewport.MaxDepth = 1;
      SWAP_CHAIN_DESC desc; //var device = dc.device;
      if (swapchain == null)
      {
        var factory = ((IDXGIDevice)device).Adapter.GetParent(typeof(IFactory).GUID) as IFactory;
        desc.BufferDesc.Width = cs.Width;
        desc.BufferDesc.Height = cs.Height;
        desc.BufferDesc.RefreshRate.Numerator = 60;
        desc.BufferDesc.RefreshRate.Denominator = 1;
        desc.BufferDesc.Format = FORMAT.B8G8R8A8_UNORM;
        desc.SampleDesc = CheckMultisample(device, desc.BufferDesc.Format, Math.Max(1, (int)(drvsettings >> 32)));
        desc.BufferUsage = BUFFERUSAGE.RENDER_TARGET_OUTPUT;
        desc.BufferCount = 1;
        desc.OutputWindow = this.Handle;
        desc.Windowed = 1;
        swapchain = factory.CreateSwapChain(device, &desc);
        factory.MakeWindowAssociation(desc.OutputWindow, MWA_NO.ALT_ENTER | MWA_NO.WINDOW_CHANGES);
      }
      else
      {
        releasebuffers(false); desc = swapchain.Desc;
        swapchain.ResizeBuffers(desc.BufferCount, cs.Width, cs.Height, desc.BufferDesc.Format, 0);
      }

      var backbuffer = swapchain.GetBuffer(0, typeof(ITexture2D).GUID);
      RENDER_TARGET_VIEW_DESC renderDesc;
      renderDesc.Format = FORMAT.B8G8R8A8_UNORM;
      renderDesc.ViewDimension = desc.SampleDesc.Count > 1 ? RTV_DIMENSION.TEXTURE2DMS : RTV_DIMENSION.TEXTURE2D;
      this.rtv = device.CreateRenderTargetView(backbuffer, &renderDesc);
      release(backbuffer);

      TEXTURE2D_DESC descDepth;
      descDepth.Width = cs.Width;
      descDepth.Height = cs.Height;
      descDepth.MipLevels = 1;
      descDepth.ArraySize = 1;
      descDepth.Format = FORMAT.D24_UNORM_S8_UINT;
      descDepth.SampleDesc.Count = desc.SampleDesc.Count;
      descDepth.SampleDesc.Quality = desc.SampleDesc.Quality;
      //descDepth.Usage = USAGE.DEFAULT;
      descDepth.BindFlags = BIND.DEPTH_STENCIL;
      //descDepth.CPUAccessFlags = 0;
      //descDepth.MiscFlags = 0;
      var tds = device.CreateTexture2D(&descDepth);
      DEPTH_STENCIL_VIEW_DESC descDSV;
      descDSV.Format = descDepth.Format;
      //descDSV.Flags = 0;
      descDSV.ViewDimension = descDepth.SampleDesc.Count > 1 ? DSV_DIMENSION.TEXTURE2DMS : DSV_DIMENSION.TEXTURE2D;
      //descDSV.Texture2D.MipSlice = 0;
      this.dsv = device.CreateDepthStencilView(tds, &descDSV);
      release(tds);
    }
    void releasebuffers(bool swp)
    {
      release(ref this.rtv);
      release(ref this.dsv);
      if (swp && swapchain != null) { Marshal.ReleaseComObject(swapchain); swapchain = null; }
    }

    public override void Refresh()
    {
      Animations?.Invoke(); if (!inval) return;
      if (this.rtv == null) sizebuffers();
      var t = TimerTicks != 0 ? Stopwatch.GetTimestamp() : 0;
      Begin(rtv, dsv, viewport, BkColor); // root != null ? root.Color : (uint)BackColor.ToArgb());
      OnRender(new DC(this)); swapchain.Present(0, 0);
      inval = false; Debug.Assert(StackPtr - BasePtr == 376 + 128);
      if (t != 0) TimerTicks = Stopwatch.GetTimestamp() - t;
    }
    public new void Invalidate()
    {
      if (inval) return;
      inval = true; Invaliated?.Invoke();
    }
    public string Adapter
    {
      get => ((IDXGIDevice)device).Adapter.Desc.Description;
    }

    static IDevice? device;
    static IDeviceContext? context;
    static VIEWPORT* currentvp; static float pixelscale;
    static void* currentdsv; //IDepthStencilView
    static Texture? texture; /*static IntPtr currentsrv; //IShaderResourceView*/
    static Font font;
    static void* cbperobject, cbperframe, cbpsperobject, cbpertexture; //IBuffer
    static cbPerObject* cb1; static cbPerFrame* cb2; static cbPsPerObject* cb3; static cbPerTexture* cb4; static int cbsok;
    struct cbPerFrame
    {
      internal Matrix4x4 ViewProjection;
      internal Vector4 Ambient;
      internal Vector4 LightDir;
    }
    struct cbPerObject
    {
      internal Matrix4x4 World;
    }
    struct cbPerTexture
    {
      internal Matrix4x4 Trans;
    }
    struct cbPsPerObject
    {
      internal Vector4 Diffuse;
    }

    static bool cmpcpy(void* d, void* s, int n)
    {
      Debug.Assert(n >> 2 << 2 == n);
      int i = 0; for (n >>= 2; i < n && ((int*)d)[i] == ((int*)s)[i]; i++) ; if (i == n) return false;
      for (; i < n; i++) ((int*)d)[i] = ((int*)s)[i]; return true;
    }
    static void Begin(void*/*IRenderTargetView*/ rtv, void*/*IDepthStencilView*/ dsv, VIEWPORT viewport, uint bkcolor)
    {
      if (cmpcpy(currentvp, &viewport, sizeof(VIEWPORT))) { context.RSSetViewports(1, &viewport); pixelscale = 0; }
      context.OMSetRenderTargets(1, &rtv, currentdsv = dsv);
      var c = ToVector4(bkcolor); context.ClearRenderTargetView(rtv, &c);
      context.ClearDepthStencilView(dsv, CLEAR.DEPTH | CLEAR.STENCIL, 1, 0);
    }

    static int mode, drvmode = 0x7fffffff, stencilref;
    static Vector4 blenfactors; static uint samplemask;
    static void* vertexlayout; //IInputLayout
    static void*[] vertexshader = new void*[4]; //IVertexShader
    static void*[] geoshader = new void*[3]; //IGeometryShader
    static void*[] pixelshader = new void*[6]; //IPixelShader
    static void*[] depthstencil = new void*[7]; //IDepthStencilState
    static void*[] blend = new void*[3]; //IBlendState
    static void*[] rasterizer = new void*[4]; //IRasterizerState
    static void*[] sampler = new void*[3]; //ISamplerState

    static void CreateVertexShader(int id)
    {
      var s = id == 0 ? vs_main_1 : id == 1 ? vs_main : id == 2 ? vs_world : vs_main_2;
      fixed (byte* str = s)
      {
        vertexshader[id] = device.CreateVertexShader(str, (UIntPtr)s.Length);
        if (vertexlayout == null)
        {
          var layout = new INPUT_ELEMENT_DESC[]
          {
                      new INPUT_ELEMENT_DESC { SemanticName = "POSITION", Format = FORMAT.R32G32B32_FLOAT, AlignedByteOffset = 0, InputSlotClass =  INPUT_CLASSIFICATION.PER_VERTEX_DATA },
                      new INPUT_ELEMENT_DESC { SemanticName = "NORMAL", Format = FORMAT.R32G32B32_FLOAT, AlignedByteOffset = 12, InputSlotClass =  INPUT_CLASSIFICATION.PER_VERTEX_DATA },
                      new INPUT_ELEMENT_DESC { SemanticName = "TEXCOORD", Format = FORMAT.R32G32_FLOAT, AlignedByteOffset = 24, InputSlotClass =  INPUT_CLASSIFICATION.PER_VERTEX_DATA },
          };
          context.IASetInputLayout(vertexlayout = device.CreateInputLayout(layout, layout.Length, str, (UIntPtr)s.Length));
        }
      }
    }
    static void CreateGeoShader(int id)
    {
      var s = id == 0 ? gs_shadows : id == 1 ? gs_outl3d : gs_line;
      fixed (byte* str = s)
        geoshader[id] = device.CreateGeometryShader(str, (UIntPtr)s.Length);
    }
    static void* CreateDepthStencilState(DepthStencil id) //IDepthStencilState
    {
      DEPTH_STENCIL_DESC ds;
      ds.DepthEnable = 1;
      ds.DepthWriteMask = DEPTH_WRITE_MASK.ZERO;
      ds.DepthFunc = COMPARISON.LESS_EQUAL;
      ds.StencilEnable = 1;
      ds.StencilReadMask = 0xff; //3D11_DEFAULT_STENCIL_READ_MASK;
      ds.StencilWriteMask = 0xff; //D3D11_DEFAULT_STENCIL_WRITE_MASK;
      ds.FrontFace.StencilFunc = COMPARISON.EQUAL;
      ds.FrontFace.StencilDepthFailOp = STENCIL_OP.KEEP;
      ds.FrontFace.StencilPassOp = STENCIL_OP.KEEP;
      ds.FrontFace.StencilFailOp = STENCIL_OP.KEEP;
      ds.BackFace = ds.FrontFace;
      switch (id)
      {
        //case DepthStencil.Default:
        //  //ds.DepthFunc = COMPARISON.ALWAYS;
        //  //ds.DepthEnable = 0; //works not for intel
        //  break;
        case DepthStencil.ZWrite:
          ds.DepthWriteMask = DEPTH_WRITE_MASK.ALL;
          break;
        case DepthStencil.StencilInc:
          ds.FrontFace.StencilPassOp = STENCIL_OP.INCR;
          break;
        case DepthStencil.StencilDec:
          ds.FrontFace.StencilPassOp = STENCIL_OP.DECR;
          break;
        case DepthStencil.ClearZ:
          ds.DepthFunc = COMPARISON.ALWAYS;
          ds.DepthWriteMask = DEPTH_WRITE_MASK.ALL;
          break;
        case DepthStencil.TwoSide:
          ds.DepthFunc = COMPARISON.LESS;
          ds.FrontFace.StencilFunc = COMPARISON.ALWAYS;
          ds.BackFace.StencilFunc = COMPARISON.ALWAYS;
          ds.FrontFace.StencilDepthFailOp = STENCIL_OP.INCR;
          ds.BackFace.StencilDepthFailOp = STENCIL_OP.DECR;
          break;
        case DepthStencil.ClearStencil:
          ds.DepthEnable = 0;
          ds.FrontFace.StencilFunc = COMPARISON.LESS;
          ds.FrontFace.StencilPassOp = STENCIL_OP.REPLACE;
          ds.BackFace = ds.FrontFace;
          break;
      }
      return device.CreateDepthStencilState(&ds);
    }
    static void* CreateBlendState(BlendState id) //IBlendState
    {
      var bd = new BLEND_DESC();
      bd.AlphaToCoverageEnable = 0;
      bd.IndependentBlendEnable = 0;
      bd.RenderTarget0.BlendEnable = 0;
      bd.RenderTarget0.SrcBlend = BLEND.ONE;
      bd.RenderTarget0.DestBlend = BLEND.ZERO;
      bd.RenderTarget0.BlendOp = BLEND_OP.ADD;
      bd.RenderTarget0.SrcBlendAlpha = BLEND.ONE;
      bd.RenderTarget0.DestBlendAlpha = BLEND.ZERO;
      bd.RenderTarget0.BlendOpAlpha = BLEND_OP.ADD;
      bd.RenderTarget0.RenderTargetWriteMask = COLOR_WRITE_ENABLE.ALL;
      switch (id)
      {
        case BlendState.Alpha:
          bd.RenderTarget0.BlendEnable = 1;
          bd.RenderTarget0.SrcBlend = BLEND.SRC_ALPHA;
          bd.RenderTarget0.DestBlend = BLEND.INV_SRC_ALPHA;
          bd.RenderTarget0.SrcBlendAlpha = BLEND.SRC_ALPHA;
          bd.RenderTarget0.DestBlendAlpha = BLEND.INV_SRC_ALPHA;
          break;
        case BlendState.AlphaAdd:
          bd.RenderTarget0.BlendEnable = 1;
          bd.RenderTarget0.SrcBlend = BLEND.ONE;
          bd.RenderTarget0.DestBlend = BLEND.ONE;
          break;
      }
      return device.CreateBlendState(&bd);
    }
    static void* CreateRasterizerState(Rasterizer id) //IRasterizerState
    {
      RASTERIZER_DESC rd;
      rd.CullMode = id == Rasterizer.CullBack ? CULL_MODE.BACK : id == Rasterizer.CullFront ? CULL_MODE.FRONT : CULL_MODE.NONE;// NONE = 1, FRONT = 2, BACK = 3
      rd.FillMode = id == Rasterizer.Wireframe ? FILL_MODE.WIREFRAME : FILL_MODE.SOLID;
      rd.DepthClipEnable = 1;
      rd.MultisampleEnable = 1;
      //rd.FrontCounterClockwise = 0; //righthand
      //rd.DepthBias = 0;
      //rd.DepthBiasClamp = 0;
      //rd.SlopeScaledDepthBias = 0;
      //rd.ScissorEnable = 0;
      //rd.AntialiasedLineEnable = 0;
      return device.CreateRasterizerState(&rd);
    }
    static void* CreateSamplerState(Sampler id) //ISamplerState
    {
      SAMPLER_DESC sd;
      sd.Filter = FILTER.MIN_MAG_MIP_LINEAR;
      sd.AddressU = TEXTURE_ADDRESS_MODE.WRAP;
      sd.AddressV = TEXTURE_ADDRESS_MODE.WRAP;
      sd.AddressW = TEXTURE_ADDRESS_MODE.WRAP;
      sd.MaxAnisotropy = 1;
      sd.ComparisonFunc = COMPARISON.ALWAYS;
      sd.MaxLOD = float.MaxValue;
      switch (id)
      {
        case Sampler.Font:
          sd.AddressU = TEXTURE_ADDRESS_MODE.BORDER;
          sd.AddressV = TEXTURE_ADDRESS_MODE.BORDER;
          break;
        case Sampler.Line:
          sd.AddressV = TEXTURE_ADDRESS_MODE.BORDER;
          break;
      }
      return device.CreateSamplerState(&sd);
    }
    static void* CreateConstBuffer(int size) //IBuffer
    {
      BUFFER_DESC desc;
      desc.Usage = USAGE.DYNAMIC;
      desc.BindFlags = BIND.CONSTANT_BUFFER;
      desc.CPUAccessFlags = CPU_ACCESS_FLAG.WRITE;
      desc.MiscFlags = 0; desc.ByteWidth = size;
      return device.CreateBuffer(&desc);
    }
    static void* CreateBuffer(void* p, int n, BIND bind) //IBuffer
    {
      BUFFER_DESC bd; SUBRESOURCE_DATA id;
      bd.Usage = USAGE.IMMUTABLE;// USAGE.DEFAULT;
      bd.ByteWidth = n;
      bd.BindFlags = bind;
      id.pSysMem = p;
      return device.CreateBuffer(&bd, &id);
    }

    static void apply()
    {
      var mask = drvmode ^ mode; drvmode = mode;
      for (int i = 0; mask != 0; i++, mask >>= 4)
      {
        if ((mask & 0xf) == 0) continue;
        var id = (mode >> (i << 2)) & 0xf;
        switch (i)
        {
          case 0: context.IASetPrimitiveTopology((PRIMITIVE_TOPOLOGY)id); continue;
          case 1:
            if (id-- == 0) { context.PSSetShader(null); continue; }
            if (pixelshader[id] == null)
            {
              var s = id == 0 ? ps_main_0 : id == 1 ? ps_main_1 : id == 2 ? ps_main_2 : id == 3 ? ps_font : id == 4 ? ps_main_a : ps_main_3;
              fixed (byte* str = s)
                pixelshader[id] = device.CreatePixelShader(str, (nint)s.Length, null);
            }
            context.PSSetShader(pixelshader[id]);
            continue;
          case 2:
            if (depthstencil[id] == null) depthstencil[id] = CreateDepthStencilState((DepthStencil)id);
            context.OMSetDepthStencilState(depthstencil[id], stencilref);
            continue;
          case 3:
            if (blend[id] == null) blend[id] = CreateBlendState((BlendState)id);
            context.OMSetBlendState(blend[id], blenfactors, ~samplemask);
            continue;
          case 4:
            if (rasterizer[id] == null) rasterizer[id] = CreateRasterizerState((Rasterizer)id);
            context.RSSetState(rasterizer[id]);
            continue;
          case 5:
            if (sampler[id] == null) sampler[id] = CreateSamplerState((Sampler)id);
            { var t = sampler[id]; context.PSSetSamplers(0, 1, &t); }
            continue;
          case 6:
            if (id-- == 0) { context.GSSetShader(null); continue; }
            if (geoshader[id] == null) CreateGeoShader(id);
            context.GSSetShader(geoshader[id]);
            continue;
          case 7:
            if (vertexshader[id] == null) CreateVertexShader(id);
            context.VSSetShader(vertexshader[id]);
            continue;
        }
      }
      if (texture != null) SetTexture(texture);
      if (cbsok == (1 | 2 | 4 | 8)) return;
      if ((cbsok & 1) == 0 && cmpcpy(&cb2[1], &cb2[0], sizeof(cbPerFrame)))
      {
        *(cbPerFrame*)context.Map(cbperframe, 0, MAP.WRITE_DISCARD, 0).pData = cb2[0];
        context.Unmap(cbperframe, 0);
      }
      if ((cbsok & 2) == 0 && cmpcpy(&cb1[1], &cb1[0], sizeof(cbPerObject)))
      {
        *(cbPerObject*)context.Map(cbperobject, 0, MAP.WRITE_DISCARD, 0).pData = cb1[0];
        context.Unmap(cbperobject, 0);
      }
      if ((cbsok & 4) == 0 && cmpcpy(&cb3[1], &cb3[0], sizeof(cbPsPerObject)))
      {
        *(cbPsPerObject*)context.Map(cbpsperobject, 0, MAP.WRITE_DISCARD, 0).pData = cb3[0];
        context.Unmap(cbpsperobject, 0);
      }
      if ((cbsok & 8) == 0 && cmpcpy(&cb4[1], &cb4[0], sizeof(cbPerTexture)))
      {
        *(cbPerTexture*)context.Map(cbpertexture, 0, MAP.WRITE_DISCARD, 0).pData = cb4[0];
        context.Unmap(cbpertexture, 0);
      }
      cbsok = (1 | 2 | 4 | 8);
    }

    static void* ringbuffer; //IBuffer
    static int rbindex, rbcount;
    static void rballoc(int nv)
    {
      release(ref ringbuffer);
      BUFFER_DESC bd;
      bd.Usage = USAGE.DYNAMIC;
      bd.ByteWidth = (rbcount = (((nv >> 11) + 1) << 11)) << 5; //64kb 2kv
      bd.BindFlags = BIND.VERTEX_BUFFER;
      bd.CPUAccessFlags = CPU_ACCESS_FLAG.WRITE;
      ringbuffer = device.CreateBuffer(&bd);
    }

    static object? curib, curvb = string.Empty, curtex;
    static void SetVertexBuffer(VertexBuffer? vb)
    {
      if (curvb == vb) return;
      int stride = 32, offs = 0; var t = vb != null ? vb.buffer : ringbuffer;
      context.IASetVertexBuffers(0, 1, &t, &stride, &offs);
      curvb = vb;
    }
    static void SetIndexBuffer(IndexBuffer? ib)
    {
      if (curib == ib) return;
      context.IASetIndexBuffer(ib != null ? ib.buffer : null, FORMAT.R16_UINT, 0);
      curib = ib;
    }
    static void SetTexture(Texture? tex) //IShaderResourceView
    {
      if (curtex == tex) return; var t = tex != null ? tex.srv : null;
      context.PSSetShaderResources(0, 1, &t); curtex = tex;
    }

    Vector4 BlendFactors
    {
      get { return blenfactors; }
      set { if (!blenfactors.Equals(value)) { blenfactors = value; drvmode |= 0x0000f000; } }
    }
    uint BlendMask
    {
      get { return samplemask; }
      set { if (samplemask != value) { samplemask = value; drvmode |= 0x0000f000; } }
    }

    #region Pick
    static void* rtvtex, dsvtex, rtvcpu, dsvcpu; //ITexture2D
    static void* rtv1, dsv1; //IRenderTargetView rtv1; IDepthStencilView dsv1;
    static int inpick;
    static object? data; static int id, id1, id2, prim;

    System.Drawing.Point point;
    object? pickdata, pickview; int pickid, pickprim, pickz;
    Matrix4x4 pickplane, picktrans; Vector3 pickp;

    static void initpixel()
    {
      TEXTURE2D_DESC td;
      td.Width = td.Height = 1;
      td.ArraySize = td.MipLevels = td.SampleDesc.Count = 1;
      td.BindFlags = BIND.RENDER_TARGET | BIND.SHADER_RESOURCE;
      td.Format = FORMAT.B8G8R8A8_UNORM;
      rtvtex = device.CreateTexture2D(&td);

      RENDER_TARGET_VIEW_DESC rdesc;
      rdesc.Format = td.Format;
      rdesc.ViewDimension = RTV_DIMENSION.TEXTURE2D;
      rtv1 = device.CreateRenderTargetView(rtvtex, &rdesc);

      td.BindFlags = BIND.DEPTH_STENCIL; // | BIND.SHADER_RESOURCE;
      td.Format = FORMAT.D24_UNORM_S8_UINT;
      dsvtex = device.CreateTexture2D(&td);

      DEPTH_STENCIL_VIEW_DESC ddesc;
      ddesc.Format = td.Format;
      ddesc.ViewDimension = DSV_DIMENSION.TEXTURE2D;
      dsv1 = device.CreateDepthStencilView(dsvtex, &ddesc);
      //Marshal.Release(dsvtex); dsvtex = IntPtr.Zero;

      td.BindFlags = 0;
      td.CPUAccessFlags = CPU_ACCESS_FLAG.READ;// | CPU_ACCESS_FLAG.WRITE;
      td.Usage = USAGE.STAGING;
      td.Format = FORMAT.B8G8R8A8_UNORM;
      rtvcpu = device.CreateTexture2D(&td, null);

      td.Format = FORMAT.D24_UNORM_S8_UINT;
      dsvcpu = device.CreateTexture2D(&td, null);
    }
    void pick(DC dc, VertexBuffer? vb, IndexBuffer? ib, int nv, ref int sv, Topology topo)
    {
      if (data == null) return; id1++;
      if (inpick == 2)
      {
        if (id2 < 0)
          switch (dc.DepthStencil)
          {
            case DepthStencil.StencilInc: id2--; break;
            case DepthStencil.StencilDec: if (id2++ == -1) { id2 = 0; pickview = data; } break;
          }
        if (topo != 0) prim = id1;
        if (id1 == id2) //capture
        {
          pickdata = data; pickid = id; pickprim = id1 - prim;
          pickplane = cb2->ViewProjection; picktrans = cb1->World; id2 = -1;
        }
        return;
      }
      if (topo != 0) dc.Topology = topo;
      var t1 = mode; var t2 = dc.Color; //todo: V4
      if (dc.PixelShader != PixelShader.Null)
      {
        if (dc.PixelShader == PixelShader.AlphaTexture) dc.PixelShader = PixelShader.Mask;
        else dc.PixelShader = PixelShader.Color;
      }
      else if (dc.DepthStencil == DepthStencil.ClearZ)
      {
        testpick(); //dc.Clear(CLEAR.DEPTH); 
        Vector4 v; context.ClearRenderTargetView(rtv1, &v);
        context.ClearDepthStencilView(dsv1, CLEAR.DEPTH, 1, 0);
        return;
      }

      dc.BlendState = BlendState.Opaque; dc.Color = unchecked((uint)id1);
      apply(); SetVertexBuffer(vb);
      if (ib == null) { context.Draw(nv, sv); sv += nv; }
      else { SetIndexBuffer(ib); context.DrawIndexed(nv, sv, 0); }
      mode = t1; dc.Color = t2;
    }
    void testpick()
    {
      context.CopyResource(rtvcpu, rtvtex);
      var pc = *(int*)context.Map(rtvcpu, 0, MAP.READ, 0).pData; context.Unmap(rtvcpu, 0);
      if (pc == 0) return;
      context.CopyResource(dsvcpu, dsvtex);
      pickz = *(int*)context.Map(dsvcpu, 0, MAP.READ, 0).pData; context.Unmap(dsvcpu, 0); id2 = pc;
    }
    void pick(System.Drawing.Point p)
    {
      if (swapchain == null) return;
      if (rtv1 == null) initpixel(); point = p;
      var vp = viewport; vp.TopLeftX = -point.X; vp.TopLeftY = -point.Y;
      Begin(rtv1, dsv1, vp, 0);
      inpick = 1; data = pickdata = pickview = null; id1 = id2 = pickid = pickprim = 0;
      var dc = new DC(this); OnRender(dc); testpick();
      pickp.X = +((point.X * 2) / viewport.Width - 1);
      pickp.Y = -((point.Y * 2) / viewport.Height - 1);
      pickp.Z = (pickz & 0xffffff) * (1.0f / 0xffffff);
      inpick = 2; id1 = 0; OnRender(dc); inpick = 0;
    }
    #endregion

    #region Native
    //static void release(ref IntPtr p) { if (p != IntPtr.Zero) { var c = Marshal.Release(p); p = IntPtr.Zero; } }
    //static void release(IntPtr[] a) { if (a != null) { for (int i = 0; i < a.Length; i++) release(ref a[i]); } }
    static void release(void* p)
    {
      if (p != null) { var c = Marshal.Release((IntPtr)p); }
    }
    static void release(ref void* p)
    {
      if (p != null) { var c = Marshal.Release((IntPtr)p); p = null; }
    }
    static void release(void*[]? a)
    {
      if (a != null)
        for (int i = 0; i < a.Length; i++) release(ref a[i]);
    }
    #endregion
  }
  //wnd
  unsafe partial class DX11Ctrl
  {
    protected override void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        case 0x0113: Refresh(); return; //WM_TIMER  
        case 0x0014: m.Result = (IntPtr)1; return; //WM_ERASEBKGND
        case 0x000F: ValidateRect(m.HWnd, null); inval = true; Refresh(); return; //WM_PAINT
        case 0x0201: //WM_LBUTTONDOWN           
        case 0x0204: //WM_RBUTTONDOWN
        case 0x0207: //WM_MBUTTONDOWN
          Capture = true; //Focus();
          pick(new System.Drawing.Point((int)m.LParam)); if (OnMouse(m.Msg, new PC(this)) != 0) return;
          break;
        case 0x0200: //WM_MOUSEMOVE
          if (tool != null) { point = new System.Drawing.Point((int)m.LParam); tool(0); Refresh(); return; }
          pick(new System.Drawing.Point((int)m.LParam)); OnMouse(m.Msg, new PC(this));
          break;
        case 0x0202: //WM_LBUTTONUP
        case 0x0205: //WM_RBUTTONUP
        case 0x0208: //WM_MBUTTONUP
          Capture = false;
          if (tool != null) { tool(1); tool = null; }
          if (OnMouse(m.Msg, new PC(this)) != 0) return;
          break;
        case 0x020A: //WM_MOUSEWHEEL
        case 0x020E: //WM_MOUSEHWHEEL
          if (tool != null) return;
          pick(PointToClient(Cursor.Position)); pickprim = (int)m.WParam.ToInt64() >> 16;
          OnMouse(m.Msg, new PC(this));
          return;
        case 0x0203: //WM_LBUTTONDBLCLK  
          OnMouse(m.Msg, new PC(this));
          return;
        case 0x02A3: //WM_MOUSELEAVE
          if (indrag) return;
          if (tool != null) { tool(1); tool = null; }
          OnMouse(m.Msg, new PC(this)); pickdata = pickview = null; pickid = 0;
          return;
        //case 0x0233: //WM_DROPFILES
        //  OnDispatch(m.Msg, new PC(this));
        //  return;
        case 0x007B: //WM_CONTEXTMENU 
          if (OnMouse(m.Msg, new PC(this)) != 0) return;
          break;
        case 0x0005: releasebuffers(false); /*Invalidate();*/ base.Invalidate(); break; //WM_SIZE
        //case 0x0020: m.Result = (IntPtr)1; return; // WM_SETCURSOR
        case 0x0100: //WM_KEYDOWN  
        case 0x0102: //WM_CHAR //case 0x0104: //WM_SYSKEYDOWN
          if (OnMouse(m.Msg | ((int)m.WParam << 16), new PC(this)) != 0) { Refresh(); return; }
          break;
        case 0x0007:  //WM_SETFOCUS
        case 0x0008:  //WM_KILLFOCUS
          OnMouse(m.Msg, new PC(this));
          return;
        case 0x0001: //WM_CREATE
          next = first; first = this;
          SetTimer(Handle, (void*)0, 30, null); break;
        case 0x0002: //WM_DESTROY    
          if (this == first) first = this.next; else for (var p = first; ; p = p.next) if (p.next == this) { p.next = this.next; break; }
          releasebuffers(true);
          break;
      }
      //Debug.WriteLine(m.Msg.ToString("X4"));
      base.WndProc(ref m);
    }

    Action<int>? tool;
    static DX11Ctrl first; DX11Ctrl next;
    bool indrag; //todo: check WM_MOUSELEAVE
    protected override void OnDragEnter(DragEventArgs e)
    {
      indrag = true; if (tool != null) { tool(1); tool = null; }
      pick(PointToClient(Cursor.Position)); var t = this.Tag; this.Tag = e.Data;
      OnMouse(0x0233, new PC(this)); this.Tag = t; //WM_DROPFILES      
      if (tool != null) e.Effect = DragDropEffects.Copy;
    }
    protected override void OnDragOver(DragEventArgs e)
    {
      if (tool == null) return;
      var p = PointToClient(Cursor.Position);
      if (p != point) { pick(p); tool(0); Refresh(); }
      e.Effect = DragDropEffects.Copy;
    }
    protected override void OnDragDrop(DragEventArgs? e)
    {
      indrag = false; if (tool == null) return;
      tool(e != null ? 1 : 2); tool = null;
      if (e != null) e.Effect = DragDropEffects.Copy;
    }
    protected override void OnDragLeave(EventArgs e)
    {
      OnDragDrop(null);
    }
    protected override bool IsInputKey(Keys keyData)
    {
      return true; // (keyData & (Keys.Control | Keys.Alt)) == 0;
    }

    public int OnDriver(object? test) //single view only!
    {
      if (test is ToolStripMenuItem item)
      {
        object unk; CreateDXGIFactory(typeof(IFactory).GUID, out unk); ADAPTER_DESC desc;
        var factory = (IFactory)unk; IAdapter adapter; var current = ((IDXGIDevice)device).Adapter.Desc.DeviceId;
        for (int t = 0; factory.EnumAdapters(t, out adapter) == 0; t++)
          item.DropDownItems.Add(new ToolStripMenuItem((desc = adapter.Desc).Description) { Tag = desc.DeviceId, Checked = desc.DeviceId == current });
        return 0;
      }
      var pi = ((IDXGIDevice)device).Adapter.Desc.DeviceId;
      if (test is not uint id) return unchecked((int)pi);
      if (pi == id) return 0; Cursor.Current = Cursors.WaitCursor;
      var tmp = Buffer.cache.Values.Select(p => p.Target).OfType<Buffer>().Select(p => (buffer: p, data: p.Reset())).ToArray();
      for (var p = DX11Ctrl.first; p != null; p = p.next) { p.releasebuffers(true); p.Invalidate(); }
      reset(null, null);
      memset(BasePtr, 0, (nint)(StackPtr - BasePtr)); rbindex = rbcount = cbsok = 0; drvmode = 0x7fffffff;
      CreateDriver((uint)test);
      foreach (var p in tmp) p.buffer.Reset(p.data);
      drvsettings = (drvsettings >> 32 << 32) | (uint)test; OnDeviceSettings(drvsettings); return 0;
    }
    public int OnSamples(object? test)
    {
      var current = swapchain != null ? swapchain.Desc.SampleDesc.Count : (int)(drvsettings >> 32);
      if (test is ToolStripMenuItem item)
      {
        SAMPLE_DESC desc; desc.Count = 1; desc.Quality = 0;
        for (int i = 1, q; i <= 16; i++)
          if (device.CheckMultisampleQualityLevels(FORMAT.B8G8R8A8_UNORM, i, out q) == 0 && q > 0)
            item.DropDownItems.Add(new ToolStripMenuItem($"{i} Samples") { Tag = i, Checked = current == i });
        return 0;
      }
      if (test is not int id) return current;
      drvsettings = (drvsettings & 0xffffffff) | ((long)(int)id << 32);
      releasebuffers(true); Invalidate(); OnDeviceSettings(drvsettings); return 0;
    }
    protected virtual void OnDeviceSettings(long dev)
    {
      //Application.UserAppDataRegistry.SetValue("drv", dev, Microsoft.Win32.RegistryValueKind.QWord);
    }
  }
  //dc
  unsafe partial class DX11Ctrl
  {
    public enum Topology { Points = 1, LineList = 2, LineStrip = 3, TriangleList = 4, TriangleStrip = 5, LineListAdj = 10, LineStripAdj = 11, TriangleListAdj = 12, TriangleStripAdj = 13, }
    public enum VertexShader { Default = 0, Lighting = 1, World = 2, Tex = 3 }
    public enum GeometryShader { Null = 0, Shadows = 1, Outline3D = 2, Line = 3 }
    public enum PixelShader { Null = 0, Color = 1, Texture = 2, AlphaTexture = 3, Font = 4, Color3D = 5, Mask = 6 }
    public enum BlendState { Opaque = 0, Alpha = 1, AlphaAdd = 2 }
    public enum Rasterizer { CullNone = 0, CullFront = 1, CullBack = 2, Wireframe = 3 }
    public enum Sampler { Default = 0, Font = 1, Line = 2 }
    public enum DepthStencil { Default = 0, ZWrite = 1, StencilInc = 2, StencilDec = 3, ClearZ = 4, TwoSide = 5, ClearStencil = 6 }
    public enum State
    {
      Default2D = (
        ((int)PixelShader.Color << 4) |
        ((int)VertexShader.Default << 28) |
        ((int)DepthStencil.Default << 8) |
        ((int)Rasterizer.CullNone << 16) |
        ((int)BlendState.Opaque << 12)),
      Default3D = (
        ((int)PixelShader.Color3D << 4) |
        ((int)VertexShader.Lighting << 28) |
        ((int)DepthStencil.ZWrite << 8) |
        ((int)Rasterizer.CullBack << 16) |
        ((int)BlendState.Opaque << 12) |
        ((int)Topology.TriangleListAdj)),
      Shadows3D = (
        ((int)PixelShader.Null << 4) |
        ((int)VertexShader.World << 28) |
        ((int)DepthStencil.TwoSide << 8) |
        ((int)Rasterizer.CullNone << 16) |
        ((int)BlendState.Opaque << 12) |
        ((int)Topology.TriangleListAdj) |
        ((int)GeometryShader.Shadows << 24)),
      Text2D = (
        ((int)PixelShader.Font << 4) |
        ((int)VertexShader.Default << 28) |
        ((int)DepthStencil.Default << 8) |
        ((int)Rasterizer.CullBack << 16) |
        ((int)BlendState.Alpha << 12) |
        ((int)Sampler.Font << 20)),
    }

    public readonly struct DC
    {
      public Vector2 Viewport
      {
        get { return *(Vector2*)&currentvp->Width; }
      }
      public Matrix4x4 Projection
      {
        get { return cb2->ViewProjection; }
        set { cb2->ViewProjection = value; cbsok &= ~1; pixelscale = 0; }
      }
      public Matrix4x4 Transform
      {
        get { return cb1->World; }
        set { cb1->World = value; cbsok &= ~2; }
      }
      public Matrix4x4 Textrans
      {
        get { return cb4->Trans; }
        set { cb4->Trans = value; cbsok &= ~8; }
      }
      public Topology Topology
      {
        get { return (Topology)(mode & 0x0000000f); }
        set { mode = (mode & ~0x0000000f) | (int)value; }
      }
      public PixelShader PixelShader
      {
        get { return (PixelShader)((mode & 0x000000f0) >> 4); }
        set { mode = (mode & ~0x000000f0) | ((int)value << 4); }
      }
      public GeometryShader GeometryShader
      {
        get { return (GeometryShader)((mode & 0x0f000000) >> 24); }
        set { mode = (mode & ~0x0f000000) | ((int)value << 24); }
      }
      public VertexShader VertexShader
      {
        get { return (VertexShader)((mode & 0x70000000) >> 28); }
        set { mode = (mode & ~0x70000000) | ((int)value << 28); }
      }
      public DepthStencil DepthStencil
      {
        get { return (DepthStencil)((mode & 0x00000f00) >> 8); }
        set { mode = (mode & ~0x00000f00) | ((int)value << 8); }
      }
      public BlendState BlendState
      {
        get { return (BlendState)((mode & 0x0000f000) >> 12); }
        set { mode = (mode & ~0x0000f000) | ((int)value << 12); }
      }
      public Rasterizer Rasterizer
      {
        get { return (Rasterizer)((mode & 0x000f0000) >> 16); }
        set { mode = (mode & ~0x000f0000) | ((int)value << 16); }
      }
      public Sampler Sampler
      {
        get { return (Sampler)((mode & 0x00f00000) >> 20); }
        set { mode = (mode & ~0x00f00000) | ((int)value << 20); }
      }
      public uint Color
      {
        get { return ToUInt(cb3->Diffuse); }
        set { cb3->Diffuse = ToVector4(value); cbsok &= ~4; }
      }
      public Vector3 Light
      {
        get { return *(Vector3*)&cb2->LightDir; }
        set { *(Vector3*)&cb2->LightDir = value; cbsok &= ~1; }
      }
      public float LightZero
      {
        get { return cb2->LightDir.W; }
        set { cb2->LightDir.W = value; cbsok &= ~1; }
      }
      public uint Ambient
      {
        get { return ToUInt(cb2->Ambient); }
        set { cb2->Ambient = ToVector4(value); cbsok &= ~1; }
      }
      public Font Font
      {
        get
        {
          if (font == null)
          {
            var t = System.Drawing.SystemFonts.MenuFont;
            font = GetFont(t.Name, t.Size * first.DeviceDpi * (1 / 120f), t.Style);
          }
          return font;
        }
        set { font = value; }
      }
      public Texture? Texture
      {
        get { return texture; }
        set { texture = value; }
      }
      public float Alpha //todo: modulate
      {
        get { return cb3->Diffuse.W; }
        //set
        //{
        //  //_cbPsPerObject->Diffuse.w = value;
        //  //BlendState = value < 1 ? BlendState.Alpha : BlendState.Default;
        //}
      }
      public float PixelScale
      {
        get
        {
          if (pixelscale == 0)
          {
            var p = &cb2->ViewProjection;
            pixelscale = MathF.Sqrt(p->M11 * p->M11 + p->M21 * p->M21 + p->M31 * p->M31) * currentvp->Width * 0.5f;
          }
          return pixelscale;
        }
      }
      public bool IsPicking { get => inpick != 0; }
      public void Select(object? data = null, int id = 0)
      {
        DX11Ctrl.data = data; DX11Ctrl.id = id;
      }
      public void SetPoint(int i, Vector3 p)
      {
        if (unchecked((uint)i) < 0x10000)
          ((Vector3*)StackPtr + 13)[i] = p;
      }
      public void DrawPoint(Vector3 p, float r)
      {
        DrawPoints(&p, 1, r);
      }
      public void DrawPoints(int np, float r)
      {
        DrawPoints((Vector3*)StackPtr + 13, np, r);
      }
      public void DrawPoints(Vector3* pp, int np, float r)
      {
        var t2 = this.Texture; this.Texture = texpt32 ??= gettex();
        var t0 = this.State;
        this.VertexShader = VertexShader.Default;
        this.PixelShader = PixelShader.AlphaTexture;
        this.BlendState = BlendState.Alpha;
        this.Rasterizer = Rasterizer.CullNone;
        this.DepthStencil = DepthStencil.ZWrite;
        //this.Textrans = Matrix4x4.Identity;
        Matrix4x4 m1; this.Operator(0x75, &m1); Matrix4x4.Invert(m1, out var m2);
        Vector2 t1; t1.X = -r; t1.Y = +r;
        for (int i = 0; i < np; i++)
        {
          var mp = ToVector3(Vector4.Transform(pp[i], m1));
          var v = this.BeginVertices(4);
          v[0].t.X = v[2].t.X = 0; v[1].t.X = v[3].t.X = 1;
          v[0].t.Y = v[1].t.Y = 0; v[2].t.Y = v[3].t.Y = 1;
          for (int t = 0; t < 4; t++, v++)
          {
            v->p.X = mp.X + (&t1.X)[(t >> 1) & 1];
            v->p.Y = mp.Y + (&t1.X)[t & 1];
            v->p.Z = mp.Z;
            *(Vector3*)&v->p = ToVector3(Vector4.Transform(*(Vector3*)&v->p, m2));
          }
          this.EndVertices(4, i == 0 ? Topology.TriangleStrip : 0);
        }
        this.State = t0; this.Texture = t2;
      }
      public void DrawLines(Vector3* pp, int np)
      {
        var vv = this.BeginVertices(np); Debug.Assert((np & 1) == 0);
        for (int i = 0; i < np; i++) vv[i].p = pp[i];
        this.EndVertices(np, Topology.LineList);
      }
      public void DrawPolyline(Vector3* pp, int np)
      {
        var vv = this.BeginVertices(np);
        for (int i = 0; i < np; i++) vv[i].p = pp[i];
        this.EndVertices(np, Topology.LineStrip);
      }
      public void DrawBox(in (Vector3 Min, Vector3 Max) box)
      {
        var vv = this.BeginVertices(16);
        vv[00].p = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
        vv[01].p = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
        vv[02].p = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
        vv[03].p = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
        vv[04].p = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
        vv[05].p = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
        vv[06].p = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
        vv[07].p = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
        vv[08].p = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
        vv[09].p = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);
        vv[10].p = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
        vv[11].p = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);
        vv[12].p = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
        vv[13].p = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
        vv[14].p = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
        vv[15].p = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
        this.EndVertices(16, Topology.LineStrip);
      }
      public void DrawPolygon(int i, int np)
      {
        DrawPolygon(((Vector3*)StackPtr + 13) + i, np);
      }
      public void DrawPolygon(Vector3* pp, int np)
      {
        var vv = this.BeginVertices(++np);
        for (int i = 0, k = np - 2; i < np; k = i++) vv[i].p = pp[k];
        this.EndVertices(np, Topology.LineStrip);
      }
      public void DrawPolygon(Vector3* pp, int np, float d)
      {
        bool open; if (open = np < 0) np = -np;
        Matrix4x4 m1, m2; this.Operator(0x75, &m1); Matrix4x4.Invert(m1, out m2);// vector.inv(&m1, &m2);//wm*vp*vm 
        var tt = (Vector3*)StackPtr;
        for (int i = 0; i < np; i++) tt[i] = ToVector3(Vector4.Transform(pp[i], m1));// vector.mul((float3*)&pp[i], &m1, &tt[i]);
        if (!open) tt[np++] = tt[0];
        var nu = 0; var uu = tt + np;
        for (int i = 0; i < np - 1; i++)
        {
          var t = Vector2.Normalize(*(Vector2*)&tt[i + 1] - *(Vector2*)&tt[i + 0]) * d;
          var r = new Vector3(-t.Y, t.X, 0);
          uu[nu++] = tt[i + 0] - r; uu[nu++] = tt[i + 0] + r;
          uu[nu++] = tt[i + 1] - r; uu[nu++] = tt[i + 1] + r;
        }
        for (int i = 0; i < nu; i++) uu[i] = ToVector3(Vector4.Transform(uu[i], m2));// vector.mul(&uu[i], &m2, &uu[i]);
        if (!open) { uu[nu++] = uu[0]; uu[nu++] = uu[1]; }
        var v = this.BeginVertices(nu);
        for (int t = 0; t < nu; t++) v[t].p = uu[t];
        this.EndVertices(nu, Topology.TriangleStrip);
      }
      public void DrawLine(Vector2 a, Vector2 b)
      {
        var vv = this.BeginVertices(2); *(Vector2*)&vv[0].p = a; *(Vector2*)&vv[1].p = b;
        this.EndVertices(2, Topology.LineStrip);
      }
      public void DrawLine(Vector3 a, Vector3 b)
      {
        var vv = this.BeginVertices(2); vv[0].p = a; vv[1].p = b;
        this.EndVertices(2, Topology.LineStrip);
      }
      public void DrawLine(Vector3 a, Vector3 b, float d)
      {
        var tt = (Vector3*)StackPtr; StackPtr = (byte*)(tt + 2);
        tt[0] = a; tt[1] = b; this.DrawPolygon(tt, -2, d);
        StackPtr = (byte*)tt;
      }
      public void DrawRect(float x, float y, float dx, float dy)
      {
        var vv = this.BeginVertices(5);
        vv[0].p.X = vv[3].p.X = x; vv[1].p.X = vv[2].p.X = x + dx;
        vv[0].p.Y = vv[1].p.Y = y; vv[2].p.Y = vv[3].p.Y = y + dy;
        vv[4].p = vv[0].p;
        this.EndVertices(5, Topology.LineStrip);
      }
      public void DrawEllipse(float x, float y, float dx, float dy)
      {
        x += (dx *= 0.5f); y += (dy *= 0.5f);
        int nv = csegs(dx, dy) + 1;
        var fa = (2 * MathF.PI) / (nv - 1);
        var vv = this.BeginVertices(nv);
        for (int i = 0; i < nv; i++)
        {
          var sc = MathF.SinCos(i * fa);
          vv[i].p.X = x + sc.Sin * dx;
          vv[i].p.Y = y + sc.Cos * dy;
        }
        this.EndVertices(nv, Topology.LineStrip);
      }
      public void DrawPath(Vector2* pp, int np, bool closed)
      {
        var nt = closed ? np + 1 : np; var vv = this.BeginVertices(nt);
        for (int i = 0; i < np; i++) *(Vector2*)&vv[i].p = pp[i]; if (closed) *(Vector2*)&vv[np].p = pp[0];
        this.EndVertices(nt, Topology.LineStrip);
      }
      public void DrawImage(Texture tex, float x, float y, float dx, float dy)
      {
        var t1 = this.Texture; this.Texture = tex;
        var t2 = this.State; this.PixelShader = PixelShader.AlphaTexture;
        var vv = this.BeginVertices(4);
        vv[0].p.X = vv[2].p.X = x; vv[1].p.X = vv[3].p.X = x + dx;
        vv[0].p.Y = vv[1].p.Y = y; vv[2].p.Y = vv[3].p.Y = y + dy;
        vv[1].t.X = vv[3].t.X = vv[2].t.Y = vv[3].t.Y = 1;
        this.EndVertices(4, Topology.TriangleStrip); this.Texture = t1; this.State = t2;
      }
      public void FillRect(float x, float y, float dx, float dy)
      {
        var vv = this.BeginVertices(4);
        vv[0].p.X = vv[2].p.X = x; vv[1].p.X = vv[3].p.X = x + dx;
        vv[0].p.Y = vv[1].p.Y = y; vv[2].p.Y = vv[3].p.Y = y + dy;
        this.EndVertices(4, Topology.TriangleStrip);
      }
      public void FillEllipse(float x, float y, float dx, float dy)
      {
        x += (dx *= 0.5f); y += (dy *= 0.5f);
        var se = csegs(dx, dy);
        var fa = (2 * MathF.PI) / se;
        var nv = se + 2;
        var vv = this.BeginVertices(nv);
        for (int i = 0, j = 0; j < nv; i++)
        {
          var sc = MathF.SinCos(i * fa);
          var u = sc.Sin * dx;
          var v = sc.Cos * dy;
          vv[j].p.X = x + u; vv[j++].p.Y = y + v;
          vv[j].p.X = x - u; vv[j++].p.Y = y + v;
        }
        this.EndVertices(nv, Topology.TriangleStrip);
      }
      public void FillRoundRect(float x, float y, float dx, float dy, float ra)
      {
        Vector2 pm, po;
        po.X = x + (pm.X = dx * 0.5f);
        po.Y = y + (pm.Y = dy * 0.5f);
        var se = csegs(ra, ra);
        var fa = (2 * MathF.PI) / (se - 2);
        var nv = se + 4;
        var vv = this.BeginVertices(nv);
        var ddy = (pm.Y - ra) * 2;
        for (int i = 0, j = 0, im = se >> 2; j < nv; i++)
        {
          var sc = MathF.SinCos(i * fa);
          var p = new Vector2(sc.Sin, sc.Cos) * ra;
          p.X += pm.X - ra;
          p.Y += pm.Y - ra; if (i > im) p.Y -= ddy;
          *(Vector2*)&vv[j].p = po - p; j++; p.X = -p.X;
          *(Vector2*)&vv[j].p = po - p; j++;
          if (i != im) continue; p.Y -= ddy; p.X = -p.X;
          *(Vector2*)&vv[j].p = po - p; j++; p.X = -p.X;
          *(Vector2*)&vv[j].p = po - p; j++;
        }
        this.EndVertices(nv, Topology.TriangleStrip);
      }
      public void FillFrame(
        float x1, float x2,
        float y1, float y2,
        float u1, float u2,
        float v1, float v2)
      {
        var vv = this.BeginVertices(4);
        vv[0].p.X = vv[2].p.X = x1;
        vv[0].p.Y = vv[1].p.Y = y1;
        vv[1].p.X = vv[3].p.X = x2;
        vv[2].p.Y = vv[3].p.Y = y2;
        vv[0].t.X = vv[2].t.X = u1;
        vv[0].t.Y = vv[1].t.Y = v1;
        vv[1].t.X = vv[3].t.X = u2;
        vv[2].t.Y = vv[3].t.Y = v2;
        this.EndVertices(4, Topology.TriangleStrip);
      }
      public void FillFrame(
        float x1, float x2, float x3, float x4,
        float y1, float y2, float y3, float y4,
        float u1, float u2, float u3, float u4,
        float v1, float v2, float v3, float v4)
      {
        var vv = this.BeginVertices(24);
        vv[00].p.X = vv[17].p.X = vv[19].p.X = vv[21].p.X = vv[23].p.X = x1;
        vv[00].p.Y = vv[01].p.Y = vv[03].p.Y = vv[05].p.Y = vv[23].p.Y = y1;
        vv[01].p.X = vv[02].p.X = vv[15].p.X = vv[16].p.X = vv[18].p.X = vv[20].p.X = vv[22].p.X = x2;
        vv[04].p.Y = vv[02].p.Y = vv[06].p.Y = vv[08].p.Y = vv[07].p.Y = vv[21].p.Y = vv[22].p.Y = y2;
        vv[03].p.X = vv[04].p.X = vv[06].p.X = vv[08].p.X = vv[10].p.X = vv[12].p.X = vv[13].p.X = vv[14].p.X = x3;
        vv[09].p.Y = vv[10].p.Y = vv[12].p.Y = vv[14].p.Y = vv[16].p.Y = vv[18].p.Y = vv[19].p.Y = vv[20].p.Y = y3;
        vv[05].p.X = vv[07].p.X = vv[09].p.X = vv[11].p.X = x4;
        vv[11].p.Y = vv[13].p.Y = vv[15].p.Y = vv[17].p.Y = y4;
        vv[00].t.X = vv[17].t.X = vv[19].t.X = vv[21].t.X = vv[23].t.X = u1;
        vv[00].t.Y = vv[01].t.Y = vv[03].t.Y = vv[05].t.Y = vv[23].t.Y = v1;
        vv[01].t.X = vv[02].t.X = vv[15].t.X = vv[16].t.X = vv[18].t.X = vv[20].t.X = vv[22].t.X = u2;
        vv[04].t.Y = vv[02].t.Y = vv[06].t.Y = vv[08].t.Y = vv[07].t.Y = vv[21].t.Y = vv[22].t.Y = v2;
        vv[03].t.X = vv[04].t.X = vv[06].t.X = vv[08].t.X = vv[10].t.X = vv[12].t.X = vv[13].t.X = vv[14].t.X = u3;
        vv[09].t.Y = vv[10].t.Y = vv[12].t.Y = vv[14].t.Y = vv[16].t.Y = vv[18].t.Y = vv[19].t.Y = vv[20].t.Y = v3;
        vv[05].t.X = vv[07].t.X = vv[09].t.X = vv[11].t.X = u4;
        vv[11].t.Y = vv[13].t.Y = vv[15].t.Y = vv[17].t.Y = v4;
        this.EndVertices(24, Topology.TriangleStrip);
      }
      public void DrawScreen(Vector3 p, int fl, Action<DC> draw) //fl 1: keep z
      {
        Matrix4x4 tm; this.Operator(0x7521, &tm); //push vp, push wm, wm*vp*vm 
        p = ToVector3(Vector4.Transform(p, tm)); //p *= tm;
        p.X = (int)p.X; p.Y = (int)p.Y; if ((fl & 1) == 0) p.Z = 0;
        this.Operator(0x6, &tm); Matrix4x4.Invert(tm, out tm); //vector.inv(&tm, &tm);//vm
        this.Projection = tm; this.Transform = Matrix4x4.CreateTranslation(p); draw(this);
        this.Operator(0x34, null); //pop wm, pop vp 
      }
      public void DrawArrow(Vector3 p, Vector3 v, float r, int s = 10)
      {
        var t1 = this.State;
        this.VertexShader = VertexShader.Lighting;
        this.PixelShader = PixelShader.Color3D;
        //this.Rasterizer = Rasterizer.CullNone;
        var fa = (2 * MathF.PI) / s++;
        var rl = 1 / v.Length();
        var r1 = new Vector3(v.Z, v.X, v.Y) * rl;
        var r2 = new Vector3(v.Y, v.Z, v.X) * rl;
        var vv = this.BeginVertices(s << 1);
        for (int i = 0; i < s; i++)
        {
          var (sin, cos) = MathF.SinCos(i * fa);
          var n = r1 * cos + r2 * sin;
          vv[(i << 1) + 0].p = p + n * r;
          vv[(i << 1) + 1].p = p + v;
          vv[(i << 1) + 0].n = vv[(i << 1) + 1].n = -n;
        }
        this.EndVertices(s << 1, Topology.TriangleStrip);
        this.State = t1;
      }
      public float Measure(string s, int n = -1)
      {
        var cx = float.MaxValue; fixed (char* p = s) this.Font.Measure(p, n != -1 ? n : s.Length, ref cx); return cx;
      }
      public void DrawText(float x, float y, string s)
      {
        var t1 = this.State;
        this.State = State.Text2D;
        fixed (char* p = s) this.Font.Draw(this, x, y, p, s.Length);
        this.State = t1;
      }

      public void DrawMesh(VertexBuffer? vb, IndexBuffer? ib, int i = 0, int n = 0)
      {
        Debug.Assert(this.Topology == Topology.TriangleListAdj);
        //this.Topology = Topology.TriangleListAdj;
        int nv = n != 0 ? n << 1 : ib.count; i <<= 1;
        if (inpick != 0) { view.pick(this, vb, ib, nv, ref i, 0); return; }
        SetVertexBuffer(vb);
        SetIndexBuffer(ib);
        apply(); context.DrawIndexed(nv, i, 0);
      }
      internal DC(DX11Ctrl p) => view = p; readonly DX11Ctrl view;

      public void Clear(CLEAR fl)
      {
        context.ClearDepthStencilView(currentdsv, fl, 1, 0);
      }
      public State State
      {
        get { return (State)mode; }
        set
        {
          if ((((int)value ^ mode) & 0x00000f00) != 0)
            switch ((DepthStencil)((mode & 0x00000f00) >> 8))//dc.DepthStencil)
            {
              case DepthStencil.StencilInc: stencilref++; drvmode |= 0x00000f00; break;
              case DepthStencil.StencilDec: stencilref--; drvmode |= 0x00000f00; break;
            }
          mode = (int)value;
        }
      }
      internal Vertex* BeginVertices(int nv)
      {
        if (rbindex + nv > rbcount) { if (nv > rbcount) rballoc(nv); rbindex = 0; }
        var map = context.Map(ringbuffer, 0, rbindex == 0 ? MAP.WRITE_DISCARD : MAP.WRITE_NO_OVERWRITE, 0);
        var vv = (Vertex*)map.pData + rbindex;
        for (int i = 0, n = nv << 2; i < n; i++) ((ulong*)vv)[i] = 0;
        return vv;
      }
      internal void EndVertices(int nv, Topology topo)
      {
        context.Unmap(ringbuffer, 0);
        if (inpick != 0) { view.pick(this, null, null, nv, ref rbindex, topo); return; }
        if (topo != 0)
        {
          if (this.PixelShader == PixelShader.Color) this.BlendState = cb3->Diffuse.W < 1 ? BlendState.Alpha : BlendState.Opaque;
          this.Topology = topo; apply();
          SetVertexBuffer(null);
        }
        context.Draw(nv, rbindex); rbindex += nv;
      }
      static Texture gettex()
      {
        return DX11Ctrl.GetTexture(32, 32, gr =>
        {
          gr.FillEllipse(System.Drawing.Brushes.Black, 1, 1, 30, 30);
          gr.FillEllipse(System.Drawing.Brushes.White, 4, 4, 30 - 6, 30 - 6);
        });
      }
      static Texture texpt32;
      int csegs(float rx, float ry)
      {
        //return 8;
        var tt = (int)(MathF.Pow(MathF.Max(MathF.Abs(rx), MathF.Abs(ry)), 0.95f) * this.PixelScale);
        return Math.Max(8, Math.Min(200, tt)) >> 1 << 1;
      }
      void Operator(int code, void* p)
      {
        for (; code != 0; code >>= 4)
          switch (code & 0xf)
          {
            case 0x1: *(Matrix4x4*)StackPtr = cb2->ViewProjection; StackPtr += sizeof(Matrix4x4); continue; //push vp
            case 0x2: *(Matrix4x4*)StackPtr = cb1->World; StackPtr += sizeof(Matrix4x4); continue; //push wm
            case 0x3: StackPtr -= sizeof(Matrix4x4); cb2->ViewProjection = *(Matrix4x4*)StackPtr; cbsok &= ~1; pixelscale = 0; continue; //pop vp
            case 0x4: StackPtr -= sizeof(Matrix4x4); cb1->World = *(Matrix4x4*)StackPtr; cbsok &= ~2; continue; //pop wm
            case 0x5:
              *(Matrix4x4*)p = cb1->World * cb2->ViewProjection;
              continue; //wm * vp
            case 0x6: //p = vm
            case 0x7: //p = p * vm
              {
                var t = (Matrix4x4*)((code & 0xf) == 6 ? p : StackPtr); *t = new Matrix4x4();
                t->M41 = +(t->M11 = currentvp->Width * +0.5f);
                t->M42 = -(t->M22 = currentvp->Height * -0.5f);
                t->M33 = t->M44 = 1; if (p != t) *(Matrix4x4*)p *= *t;// vector.mul((float4x4*)p, t, (float4x4*)p);
              }
              continue;
            case 0x8: //wm = p * wm
              {
                //var t = (float3x4*)StackPtr; vector.conv(&cb1->World, t); vector.mul((float3x4*)p, t, t + 1); vector.conv(t + 1, &cb1->World); cbsok &= ~2;
                cb1->World = *(Matrix4x4*)p = cb1->World; cbsok &= ~2;
              }
              continue;
            case 0x9: //conv float2
              {
                var t = (Matrix4x4*)StackPtr + 2;
                *t = Matrix4x4.CreateTranslation((*(Vector2*)p).X, (*(Vector2*)p).Y, 0);
                p = t;
              }
              continue;
          }
      }
      void PushTransform(Matrix4x4 m)
      {
        Operator(0x82, &m); //push wm, wm = m * wm
      }
      void PushTransform(Vector2 trans)
      {
        Operator(0x892, &trans); //push wm, conv float2, wm = m * wm
      }
      void PopTransform()
      {
        Operator(0x4, null); //pop wm 
      }
    }
    public readonly struct PC
    {
      public DX11Ctrl View
      {
        get => view;
      }
      public Matrix4x4 Plane
      {
        get { return view.pickplane; }
        set { view.pickplane = value; }
      }
      public Matrix4x4 Transform
      {
        get { return view.picktrans; }
      }
      public object? Hover
      {
        get { return view.pickdata; }
      }
      public int Id
      {
        get { return view.pickid; }
      }
      public int Primitive
      {
        get { return view.pickprim; }
      }
      public Vector3 Point
      {
        get
        {
          if (view.pickdata == null) return default;
          Matrix4x4.Invert(view.picktrans * view.pickplane, out var m);
          return ToVector3(Vector4.Transform(view.pickp, m));
        }
      }
      public void SetTool(Action<int>? tool)
      {
        view.tool = tool;
      }
      public void SetPlane(Matrix4x4 m)
      {
        view.pickplane = m * view.pickplane;
      }
      public Vector2 Pick()
      {
        Vector2 p; var vp = view.point;
        p.X = +((vp.X * 2) / currentvp->Width - 1);
        p.Y = -((vp.Y * 2) / currentvp->Height - 1);
        var m = view.pickplane;
        var a1 = p.X * m.M14 - m.M11;
        var b1 = p.Y * m.M14 - m.M12;
        var a2 = p.X * m.M24 - m.M21;
        var b2 = p.Y * m.M24 - m.M22;
        var de = 1 / (a1 * b2 - a2 * b1);
        var c1 = m.M41 - p.X * m.M44;
        var c2 = m.M42 - p.Y * m.M44;
        p.X = (c1 * b2 - a2 * c2) * de;
        p.Y = (a1 * c2 - c1 * b1) * de;
        return p;
      }
      public void Invalidate()
      {
        view.Invalidate();
      }
      internal PC(DX11Ctrl p) => view = p;
      readonly DX11Ctrl view;
    }

    public Bitmap Print(int dx, int dy, int samples, uint bkcolor, Action<DC> print)
    {
      TEXTURE2D_DESC td;
      td.Width = dx; td.Height = dy;
      td.ArraySize = td.MipLevels = 1;
      td.BindFlags = BIND.RENDER_TARGET | BIND.SHADER_RESOURCE;
      td.Format = FORMAT.B8G8R8A8_UNORM;
      td.SampleDesc = CheckMultisample(device, td.Format, Math.Max(1, samples));
      var tex = device.CreateTexture2D(&td);

      RENDER_TARGET_VIEW_DESC rdesc;
      rdesc.Format = td.Format;
      rdesc.ViewDimension = td.SampleDesc.Count > 1 ? RTV_DIMENSION.TEXTURE2DMS : RTV_DIMENSION.TEXTURE2D;
      var rtv = device.CreateRenderTargetView(tex, &rdesc);

      td.BindFlags = BIND.DEPTH_STENCIL;
      td.Format = FORMAT.D24_UNORM_S8_UINT;
      var ds = device.CreateTexture2D(&td);

      DEPTH_STENCIL_VIEW_DESC ddesc;
      ddesc.Format = td.Format;
      ddesc.ViewDimension = td.SampleDesc.Count > 1 ? DSV_DIMENSION.TEXTURE2DMS : DSV_DIMENSION.TEXTURE2D;
      var dsv = device.CreateDepthStencilView(ds, &ddesc);

      VIEWPORT viewport; (&viewport)->Width = dx; viewport.Height = dy; viewport.MaxDepth = 1;
      Begin(rtv, dsv, viewport, bkcolor);

      print(new DC(this));

      void* zero = null; context.OMSetRenderTargets(1, &zero, zero);
      release(dsv); release(ds); release(rtv);

      td.BindFlags = 0;
      td.Format = FORMAT.B8G8R8A8_UNORM;
      if (td.SampleDesc.Count > 1)
      {
        td.SampleDesc.Count = 1;
        td.SampleDesc.Quality = 0;
        td.Usage = USAGE.DEFAULT;
        var t1 = device.CreateTexture2D(&td);
        context.ResolveSubresource(t1, 0, tex, 0, td.Format);
        release(tex); tex = t1;
      }

      td.Usage = USAGE.STAGING;
      td.CPUAccessFlags = CPU_ACCESS_FLAG.READ;
      var t2 = device.CreateTexture2D(&td);
      context.CopyResource(t2, tex);
      release(tex); tex = t2;

      var map = context.Map(tex, 0, MAP.READ, 0);
      var a = new Bitmap(dx, dy, map.RowPitch, System.Drawing.Imaging.PixelFormat.Format32bppArgb, new IntPtr(map.pData));
      var b = new Bitmap(a); a.Dispose();
      context.Unmap(tex, 0); release(tex);
      return b;
    }

    static Vector3 ToVector3(Vector4 p)
    {
      return new Vector3(p.X, p.Y, p.Z) / p.W;
    }
    static uint ToUInt(Vector4 s)
    { //todo: SSE
      s *= 255; return unchecked((byte)s.Z | ((uint)(byte)s.Y << 8) | ((uint)(byte)s.X << 16) | ((uint)(byte)s.X << 24));
    }
    static Vector4 ToVector4(uint s)
    { //todo: SSE
      return new Vector4(((s >> 16) & 0xff), ((s >> 8) & 0xff), (s & 0xff), ((s >> 24) & 0xff)) * (1.0f / 255);
    }
    static System.Text.Encoding encoding = System.Text.Encoding.UTF8;
    static void WriteCount(ref Span<byte> ws, int count)
    {
      var u = unchecked((uint)count); int i = 0;
      for (; u >= 0x80; u >>= 7) ws[i++] = unchecked((byte)(u | 0x80));
      ws[i++] = unchecked((byte)u); ws = ws.Slice(i);
    }
    static void ReadCount(ref ReadOnlySpan<byte> rs, out int count)
    {
      int i = 0; uint u = 0;
      for (int s = 0; ; s += 7) { uint b = rs[i++]; u |= (b & 0x7F) << s; if ((b & 0x80) == 0) break; }
      rs = rs.Slice(i); count = unchecked((int)u);
    }
    static void Write(ref Span<byte> ws, string? s)
    {
      if (s == null) { WriteCount(ref ws, 0); return; }
      var n = encoding.GetByteCount(s); WriteCount(ref ws, n + 1);
      ws = ws.Slice(encoding.GetBytes(s, ws));
    }
    static void Read(ref ReadOnlySpan<byte> rs, out string? s)
    {
      ReadCount(ref rs, out var n); if (n-- <= 0) { s = n == 0 ? string.Empty : null; return; }
      s = encoding.GetString(rs.Slice(0, n)); rs = rs.Slice(n);
    }
    static void Write<T>(ref Span<byte> ws, T v) where T : unmanaged
    {
      var rs = new ReadOnlySpan<byte>((byte*)&v, sizeof(T));
      rs.CopyTo(ws); ws = ws.Slice(rs.Length);
    }
    static void Read<T>(ref ReadOnlySpan<byte> rs, out T v) where T : unmanaged
    {
      var ss = rs.Slice(0, sizeof(T)); rs = rs.Slice(ss.Length);
      fixed (T* p = &v) ss.CopyTo(new Span<byte>(p, ss.Length));
    }
  }
  //buffer
  unsafe partial class DX11Ctrl
  {
    public abstract class Buffer
    {
      uint refcount, id;
      internal static Dictionary<uint, GCHandle> cache = new Dictionary<uint, GCHandle>();
      internal static Buffer GetBuffer(Type type, void* p, int n, Buffer? buffer)
      {
        uint idfree = 0, id = 0; //for (int t = 0, l = n >> 2; t < l; t++) id = id * 31 + ((uint*)p)[t];
        for (int i = 0, c = n >> 2; i < c; i++) id = ((id << 7) | (id >> 25)) ^ ((uint*)p)[i];
        if (id == 0) id = 13;
        for (; ; id++)
        {
          if (!cache.TryGetValue(id, out var h)) break;
          var test = h.Target as Buffer;
          if (test == null) { idfree = id; continue; }
          if (test.GetType() != type) continue;
          if (!test.Equals(p, n)) continue;
          test.AddRef(); buffer?.Release(); return test;
        }
        if (buffer == null || !buffer.Release()) buffer = (Buffer)Activator.CreateInstance(type);
        if (idfree != 0) { var h = cache[buffer.id = idfree]; h.Target = buffer; }
        else cache.Add(buffer.id = id, GCHandle.Alloc(buffer, GCHandleType.Weak));
        buffer.Init(p, n); buffer.AddRef(); return buffer;
      }
      public void AddRef()
      {
        refcount++;
      }
      public bool Release()
      {
        Debug.Assert(refcount > 0);
        if (--refcount != 0) return false;
        var h = cache[id]; Debug.Assert(id != 0);
        if (cache.ContainsKey(id + 1)) h.Target = null;
        else { h.Free(); cache.Remove(id); }
        id = 0; Dispose(); return true;
      }
      ~Buffer()
      {
        Dispose();
      }
      protected abstract bool Equals(void* p, int n);
      protected abstract void Init(void* p, int n);
      protected abstract void Dispose();
      protected abstract int GetData(void* p);
      internal byte[] Reset() { var n = GetData(StackPtr); var a = new byte[n]; memcpy(a, StackPtr); Dispose(); return a; }
      internal void Reset(byte[] a) { fixed (void* t = a) Init(t, a.Length); }
      public static IEnumerable<(int Count, Type? Type)> CacheState
      {
        get => cache.GroupBy(p => p.Value.Target is object b ? b.GetType() : null).Select(p => (p.Count(), p.Key));
      }
    }

    static void copy(void* p, int n, void* buffer)
    {
      BUFFER_DESC bd;
      bd.ByteWidth = n;
      bd.CPUAccessFlags = CPU_ACCESS_FLAG.READ;
      bd.BindFlags = BIND.SHADER_RESOURCE;
      var tmp = device.CreateBuffer(&bd);
      context.CopyResource(tmp, buffer);
      var map = context.Map(tmp, 0, MAP.READ, 0);
      memcpy(p, map.pData, bd.ByteWidth);
      context.Unmap(tmp, 0); release(tmp);
    }
    static bool equals(void* buffer, void* p, int n)
    {
      BUFFER_DESC bd; bd.ByteWidth = n;
      bd.CPUAccessFlags = CPU_ACCESS_FLAG.READ;
      bd.BindFlags = BIND.SHADER_RESOURCE;
      var tmp = device.CreateBuffer(&bd);
      context.CopyResource(tmp, buffer);
      var map = context.Map(tmp, 0, MAP.READ, 0);
      n = memcmp(p, map.pData, n);
      context.Unmap(tmp, 0); release(tmp);
      return n == 0;
    }

    public sealed class VertexBuffer : Buffer
    {
      internal void* buffer; //IBuffer 
      internal int count;
      protected override void Dispose()
      {
        release(ref buffer);
      }
      protected override bool Equals(void* p, int n)
      {
        if (count == 0) return n == 0;
        if (n != count * sizeof(Vertex)) return false;
        return equals(buffer, p, n);
      }
      protected override void Init(void* p, int n)
      {
        Debug.Assert(buffer == null);
        if (n == 0) { n = 1; p = (byte*)&n + 2; }
        count = n / sizeof(Vertex);
        buffer = CreateBuffer(p, n, BIND.VERTEX_BUFFER);
      }
      protected override int GetData(void* p)
      {
        copy(p, count * sizeof(Vertex), buffer);
        return count * sizeof(Vertex);
        //return GetVertices((Vertex*)p) * sizeof(Vertex);
      }
      //internal int GetVertices(Vertex* p) //todo: remove, check normals?
      //{
      //  copy(p, count * sizeof(Vertex), buffer); return count;
      //}
    }

    public sealed class IndexBuffer : Buffer
    {
      internal void* buffer; //IBuffer 
      internal int count;
      protected override void Dispose()
      {
        release(ref buffer);
      }
      protected override bool Equals(void* p, int n)
      {
        if (count == 0) return n == 0;
        if (n != count * sizeof(ushort)) return false;
        return equals(buffer, p, n);
      }
      protected override void Init(void* p, int n)
      {
        Debug.Assert(buffer == null);
        if (n == 0) { n = 1; p = (byte*)&n + 2; }
        count = n / sizeof(ushort);
        buffer = CreateBuffer(p, n, BIND.INDEX_BUFFER);
      }
      protected override int GetData(void* p)
      {
        copy(p, count * sizeof(ushort), buffer);
        return count * sizeof(ushort);
      }
    }

    //[ThreadStatic]
    //static bool IsMainThread = true;

    public sealed class Texture : Buffer
    {
      internal void* srv; /*D3D11.IShaderResourceView*/
      byte[] data; int info; Action<Graphics>? draw;
      public Vector2 Size
      {
        get { return new Vector2(info & 0xffff, info >> 16); }
      }
      public string? Url
      {
        get
        {
          if (data == null) return null;
          var rs = new ReadOnlySpan<byte>(data); Read(ref rs, out string url);
          return url;
        }
      }
      protected override void Init(void* p, int n)
      {
        Debug.Assert(srv == null);
        memcpy(data ??= new byte[n], p); //srv = CreateTexture((byte*)p, n, out info);
        var rs = new ReadOnlySpan<byte>(p, n); Read(ref rs, out string url);
        try
        {
          if (url == null)
          {
            if (draw == null) draw = (Action<Graphics>)DX11Ctrl.data;
            ReadCount(ref rs, out var dx); ReadCount(ref rs, out var dy);
            using (var bmp = new Bitmap(dx, dy, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
              using (var gr = Graphics.FromImage(bmp))
              {
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                draw(gr);
              }
              //if (fmt != 0)
              //{
              //  var data = bmp.LockBits(new Rectangle(0, 0, dx, dy), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
              //  var ptr = (byte*)data.Scan0.ToPointer(); Debug.Assert(data.Stride == dx << 2);
              //  var n = data.Stride * dy;
              //  if (fmt == FORMAT.A8_UNORM) { for (int i = 3; i < n; i += 4) ptr[i >> 2] = ptr[i]; n >>= 2; }
              //  var i1 = StackPtr; var ii = (int*)i1; ii[0] = (int)fmt << 16; ii[1] = dx | (dy << 16);
              //  StackPtr = i1 + 8; Zip.Compress(ptr, n); bmp.UnlockBits(data);
              //  var tex = (Texture)Buffer.GetBuffer(typeof(Texture), ii, (int)(StackPtr - i1));
              //  StackPtr = i1; return;
              //}
              info = bmp.Width | (bmp.Height << 16);
              srv = CreateTexture(bmp);
            }
            return;
          }
          static void load(Texture p, string s)
          {
            using (var bmp = Image.FromFile(s))
            {
              p.info = bmp.Width | (bmp.Height << 16);
              p.srv = CreateTexture((System.Drawing.Bitmap)bmp);
            }
          }
          if (url.IndexOf(':', 0, 8) <= 1) { load(this, url); return; }
          var uri = new Uri(url);
          if (uri.IsFile) { load(this, uri.LocalPath); return; }
          try
          {
            var ci = (INTERNET_CACHE_ENTRY_INFO*)StackPtr; int cn = 4096;
            var ok = GetUrlCacheEntryInfo(url, ci, ref cn);
            if (ok) { load(this, new string((char*)ci->path)); return; }
          }
          catch (Exception ex) { Debug.WriteLine(ex.Message); }
          var client = GetHttpClient();
          using (var task = client.GetByteArrayAsync(uri))
          {
            task.Wait();
            if (task.Status == TaskStatus.RanToCompletion)
            {
              using (var str = new MemoryStream(task.Result))
              using (var bmp = Image.FromStream(str))
              {
                info = bmp.Width | (bmp.Height << 16);
                srv = CreateTexture((System.Drawing.Bitmap)bmp);
              }
              var ext = Path.GetExtension(url);
              var ok = CreateUrlCacheEntry(url, 0, ext, (char*)StackPtr, 0);
              if (ok)
              {
                var path = new string((char*)StackPtr);
                File.WriteAllBytes(path, task.Result);
                const string h = "HTTP/1.0 200 OK\r\n\r\n";
                ok = CommitUrlCacheEntry(url, path, default, default, 1, h, (uint)h.Length, ext, null);
                Debug.Assert(ok);
              }
            }
          }
        }
        catch (Exception e) { Debug.WriteLine(url + " " + e.Message); }
      }
      protected override bool Equals(void* p, int n)
      {
        if (data.Length != n) return false;
        fixed (byte* t = data) return memcmp(t, p, n) == 0;
      }
      protected override int GetData(void* p)
      {
        memcpy(p, data); return data.Length;
      }
      protected override void Dispose()
      {
        release(ref srv);
      }
    }

    //static WeakReference http;
    //protected static HttpClient GetHttpClient()
    //{
    //  if (http == null || http.Target is not HttpClient client)
    //    http = new WeakReference(client = new HttpClient());
    //  return client;
    //}
    static HttpClient http;
    protected static HttpClient GetHttpClient() => http ??= new HttpClient();

    static void* CreateTexture(Bitmap bmp, int flags = 0) //IShaderResourceView
    {
      int dx = bmp.Width, dy = bmp.Height;
      var pf = bmp.PixelFormat; FORMAT fmt;
      switch (pf)
      {
        case System.Drawing.Imaging.PixelFormat.Format32bppRgb: fmt = FORMAT.B8G8R8X8_UNORM; break;
        case System.Drawing.Imaging.PixelFormat.Format32bppArgb: fmt = FORMAT.B8G8R8A8_UNORM; if ((flags & 1) != 0) fmt = FORMAT.A8_UNORM; break;
        case System.Drawing.Imaging.PixelFormat.Format24bppRgb: fmt = FORMAT.B8G8R8X8_UNORM; pf = System.Drawing.Imaging.PixelFormat.Format32bppRgb; break;
        case System.Drawing.Imaging.PixelFormat.Format16bppRgb565: fmt = FORMAT.B5G6R5_UNORM; break;
        case System.Drawing.Imaging.PixelFormat.Format16bppRgb555: fmt = FORMAT.B5G6R5_UNORM; pf = System.Drawing.Imaging.PixelFormat.Format16bppRgb565; break;
        default: fmt = FORMAT.B8G8R8X8_UNORM; pf = System.Drawing.Imaging.PixelFormat.Format32bppRgb; break;
      }
      var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, dx, dy), System.Drawing.Imaging.ImageLockMode.ReadOnly, pf);
      var ptr = (byte*)data.Scan0.ToPointer(); var stride = data.Stride;
      if (fmt == FORMAT.A8_UNORM)
      {
        var p = ptr;
        for (int y = 0; y < dx; y++, p += stride)
          for (int x = 0; x < dy; x++) p[x] = p[(x << 2) + 3];
      }
      var srv = CreateTexture(dx, dy, stride, fmt, 1, ptr);
      bmp.UnlockBits(data); return srv;
    }
    static void* CreateTexture(int dx, int dy, int pitch, FORMAT fmt, int mips, void* p) //IShaderResourceView
    {
      if (pitch == 0) pitch = fmt == FORMAT.A8_UNORM ? dx : dx << 2;
      TEXTURE2D_DESC td; SHADER_RESOURCE_VIEW_DESC rv;
      td.Width = dx;
      td.Height = dy;
      td.ArraySize = td.SampleDesc.Count = 1;// = td.MipLevels = 1;
      td.BindFlags = BIND.SHADER_RESOURCE;
      td.Format = fmt;
      if (mips != 0)
      {
        td.BindFlags |= BIND.RENDER_TARGET;
        td.MiscFlags = RESOURCE_MISC.GENERATE_MIPS;
      }
      else td.MipLevels = 1;
      var tex = device.CreateTexture2D(&td);
      context.UpdateSubresource(tex, 0, null, p, pitch, 0);
      td = ((ITexture2D)Marshal.GetObjectForIUnknown((IntPtr)tex)).Desc;
      rv.ViewDimension = SRV_DIMENSION.TEXTURE2D;
      rv.Format = td.Format;
      rv.Texture2D.MipLevels = td.MipLevels;
      var srv = device.CreateShaderResourceView(tex, &rv);
      release(ref tex);
      if (mips != 0) context.GenerateMips(srv);
      return srv;
    }

    public static Texture GetTexture(string url)
    {
      var sp = new Span<byte>(StackPtr, 0x10000);
      var wp = sp; Write(ref wp, url);
      return (Texture)Buffer.GetBuffer(typeof(Texture), StackPtr, sp.Length - wp.Length, null);
    }
    public static Texture GetTexture(int dx, int dy, Action<Graphics> draw)
    {
      var sp = new Span<byte>(StackPtr, 0x10000); var wp = sp;
      Write(ref wp, (string)null); WriteCount(ref wp, dx); WriteCount(ref wp, dy); Write(ref wp, draw.Method.Name);
      var t1 = DX11Ctrl.data; DX11Ctrl.data = draw;
      try { return (Texture)Buffer.GetBuffer(typeof(Texture), StackPtr, sp.Length - wp.Length, null); }
      finally { DX11Ctrl.data = t1; }
    }

    public new sealed class Font : Buffer
    {
      byte[] data;
      protected override void Init(void* p, int n)
      {
        memcpy(data = new byte[n], p); create();
      }
      protected override bool Equals(void* p, int n)
      {
        if (data.Length != n) return false;
        fixed (byte* t = data) return memcmp(t, p, n) == 0;
      }
      protected override void Dispose()
      {
        Marshal.FreeCoTaskMem((IntPtr)glyphs); glyphs = null;
        if (hfont != null) { DeleteObject(hfont); hfont = null; }
        release(srvs); srvs = null; dict = null;
      }
      protected override int GetData(void* p)
      {
        memcpy(p, data); return data.Length;
      }
      public void PreInit(string chars)
      {
        fixed (char* s = chars) create(s, chars.Length);
      }
      public string? Name { get { return name; } }
      public float Size
      {
        get { return size; }
        //set { size = value; }
      }
      public float Ascent
      {
        get { return ascent; }
      }
      public float Descent
      {
        get { return descent; }
      }
      public float Height
      {
        get { return ascent + descent; }
      }
      //public float Height2 { get { return height; } }
      //public float IntLeading { get { return intlead; } }
      //public float ExtLeading { get { return extlead; } }
      public int Measure(char* s, int n, ref float cx)
      {
        var po = SelectObject(wdc, hfont);
        var re = default(GCP_RESULTS); re.lStructSize = sizeof(GCP_RESULTS);
        re.lpDx = (int*)StackPtr; re.nGlyphs = n;
        var hr = GetCharacterPlacementW(wdc, s, n, 0, &re, 0x0008); // GCP_USEKERNING
        SelectObject(wdc, po);
        int i = 0; float x = 0, dx;
        for (; i < n; i++) { dx = re.lpDx[i]; if (x + dx > cx) break; x += dx; }
        cx = x; return i;

        //float x = 0, dx; int i = 0;
        //for (; i < n; i++)
        //{
        //  var c = s[i]; int j; if (!dict.TryGetValue(c, out j)) { create(s + i, n - i); j = dict[c]; }
        //  dx = glyphs[j].incx * size;
        //  if (i != 0) { float k; if (kern.TryGetValue((((uint)c << 16) | s[i - 1]), out k)) dx += k * size; }
        //  if (x + dx > cx) break; x += dx;
        //}
        //cx = x; return i;
      }
      public void Draw(DC dc, float x, float y, char* s, int n)
      {
        if (inpick != 0) return;
        var po = SelectObject(wdc, hfont);
        var re = default(GCP_RESULTS); re.lStructSize = sizeof(GCP_RESULTS);
        re.lpDx = (int*)StackPtr; re.nGlyphs = n; StackPtr = (byte*)(re.lpDx + n);
        GetCharacterPlacementW(wdc, s, n, 0, &re, 0x0008); // GCP_USEKERNING
        SelectObject(wdc, po);
        var t1 = texture; texture = null;
        var topo = Topology.TriangleStrip;
        for (int a = 0, b = 0, j, bsrv = -1, csrv = -1, nc = 0; ; b++)
        {
          if (b < n)
          {
            if (!dict.TryGetValue(s[b], out j)) { create(s + b, n - b); j = dict[s[b]]; }
            bsrv = glyphs[j].srv;
          }
          if (b == n || (bsrv != csrv && bsrv != -1) || nc == 64) //256 * sizeof(vertex) 8k blocks
          {
            if (nc != 0)
            {
              var vv = dc.BeginVertices(nc << 2);
              for (; a < b; a++)
              {
                //if (a != 0) { float k; if (kern.TryGetValue((((uint)s[a] << 16) | s[a - 1]), out k)) x += k; }
                var pg = glyphs + dict[s[a]];
                if (pg->srv != -1)
                {
                  vv[0].p.X = vv[2].p.X = x + pg->orgx;
                  vv[0].p.Y = vv[1].p.Y = y - pg->orgy;
                  vv[1].p.X = vv[3].p.X = vv[0].p.X + pg->boxx;
                  vv[3].p.Y = vv[2].p.Y = vv[0].p.Y + pg->boxy;
                  vv[0].t.X = vv[2].t.X = pg->x1;
                  vv[1].t.X = vv[3].t.X = pg->x2;
                  vv[2].t.Y = vv[3].t.Y = 1; vv += 4;
                }
                x += re.lpDx[a]; //x += pg->incx;
              }
              dc.EndVertices(nc << 2, topo); topo = 0;
            }
            if (b == n) break;
            if (bsrv != -1 && csrv != bsrv)
            {
              var t = srvs[csrv = bsrv];
              context.PSSetShaderResources(0, 1, &t); curtex = this;
            }
            nc = 0;
          }
          if (bsrv != -1) nc++;
        }
        texture = t1;
        StackPtr = (byte*)re.lpDx;
      }

      string? name; float size, ascent, descent; void* hfont;
      Dictionary<char, int>? dict; //Dictionary<uint, float> kern;
      struct Glyph { internal float boxx, boxy, orgx, orgy, incx, x1, x2; internal int srv; }
      Glyph* glyphs; int glyphn, glypha;
      void*[]? srvs; int srvn; /*D3D11.IShaderResourceView*/
      void create()
      {
        var rs = new ReadOnlySpan<byte>(data);
        Read(ref rs, out name); Read(ref rs, out float size); Read(ref rs, out FontStyle style);
        this.size = size;
        fixed (char* s = name) hfont = CreateFontW(-(int)(size * 1.75), 0, 0, 0,
            (style & FontStyle.Bold) != 0 ? 700 : 400,
            (style & FontStyle.Italic) != 0 ? 1 : 0,
            (style & FontStyle.Underline) != 0 ? 1 : 0,
            (style & FontStyle.Strikeout) != 0 ? 1 : 0,
            1, //DEFAULT_CHARSET
            4, //OUT_TT_PRECIS
            0,
            5, //CLEARTYPE_QUALITY
            0, s);
        if (wdc == null) wdc = GetDC(null);
        var po = SelectObject(wdc, hfont);
        int* textmetric = (int*)StackPtr;
        GetTextMetrics(wdc, textmetric);
        SelectObject(wdc, po);
        ascent = textmetric[1];
        descent = textmetric[2];
        //height = textmetric[0] * scale;
        //intlead = textmetric[3] * scale;
        //extlead = textmetric[4] * scale;
        dict = new Dictionary<char, int>(128);
      }
      void create(char* s, int n)
      {
        const int ex = 4;
        int nc = 0; var cc = (char*)StackPtr;
        for (int i = 0; i < n; i++)
        {
          var c = s[i]; if (dict.ContainsKey(c)) continue;
          if (!dict.ContainsKey(c)) { dict[c] = -1; cc[nc++] = c; }
        }
        if (nc == 0) return;
        //for (int i = 0; i < nc; i++) Debug.Write(cc[i] + " "); Debug.WriteLine("");
        if (glyphs == null || glyphn + nc > glypha)
        {
          var cb = (glypha = (((glyphn + nc) >> 5) + 1) << 5) * sizeof(Glyph);
          glyphs = (Glyph*)Marshal.ReAllocCoTaskMem((IntPtr)glyphs, cb).ToPointer();// realloc(glyphs, cb);
        }
        var ma = default(Matrix4x4); var m2 = (int*)&ma; m2[0] = m2[3] = 0x10000;
        var po = SelectObject(wdc, hfont);
        for (int i1 = 0, i2 = 0, gi = glyphn; i1 < nc; i1 = i2)
        {
          int mx = ex, my = 0;
          for (int i = i1; i < nc; i++, i2++)
          {
            var gm = (int*)&glyphs[gi + i]; GetGlyphOutlineW(wdc, cc[i], 0, gm, 0, null, m2); if (cc[i] <= ' ') continue;
            if (mx + gm[0] + (ex << 1) > 4096 && n > 1) break; //D3D11_REQ_TEXTURE2D_U_OR_V_DIMENSION (16384)
            mx += gm[0] + (ex << 1); my = Math.Max(my, gm[1] + (ex << 1));
          }
          if (my != 0)
          {
            if (srvs == null || srvn == srvs.Length)
            {
              var a = new void*[srvn == 0 ? 4 : srvn << 1];
              if (srvs != null) for (int i = 0; i < srvs.Length; i++) a[i] = srvs[i];
              srvs = a; //Array.Resize(ref srvs, srvn == 0 ? 4 : srvn << 1);
            }
            byte* pp = null; var bi = m2 + 4; bi[0] = 40; bi[1] = mx; bi[2] = -my; bi[3] = 1 | (32 << 16);
            var dib = CreateDIBSection(null, bi, 0, &pp, null, 0);
            var ddc = CreateCompatibleDC(wdc);
            var obmp = SelectObject(ddc, dib);
            var ofont = SelectObject(ddc, hfont);
            //int old = SetMapMode(ddc, 1); //MM_TEXT
            SetTextColor(ddc, 0x00ffffff); SetBkMode(ddc, 1); SetTextAlign(ddc, 24);
            for (int i = i1, x = ex; i < i2; i++)
            {
              var c = cc[i]; if (c <= ' ') continue; var gm = (int*)&glyphs[gi + i];
              TextOutW(ddc, x - gm[2], ex + gm[3], &c, 1); x += gm[0] + (ex << 1);
            }
            SelectObject(ddc, ofont);
            SelectObject(ddc, obmp);
            DeleteDC(ddc);
            for (int k = 0, nk = mx * my; k < nk; k++)
              pp[k] = (byte)((pp[(k << 2)] * 0x4c + pp[(k << 2) + 1] * 0x95 + pp[(k << 2) + 2] * 0x1e) >> 8);
            srvs[srvn++] = CreateTexture(mx, my, mx, FORMAT.A8_UNORM, 1, pp);
            DeleteObject(dib);
          }

          for (int i = i1, x = ex; i < i2; i++)
          {
            var c = cc[i]; dict[c] = glyphn;
            var gl = &glyphs[glyphn++]; var gm = (int*)gl;
            if (c > ' ')
            {
              gl->srv = srvn - 1;
              gl->x1 = (x - ex) / (float)mx;
              gl->x2 = (x + gm[0] + ex) / (float)mx;
              x += gm[0] + (ex << 1);
            }
            else gl->srv = -1;
            gl->boxx = gm[0] + (ex << 1);
            gl->boxy = my;
            gl->orgx = gm[2] - ex;
            gl->orgy = gm[3] + ex;
            gl->incx = gm[4] & 0xffff; if (c < ' ') gl->incx *= 0.5f;
          }
        }
        SelectObject(wdc, po); //ReleaseDC(null, hdc);
      }
    }

    public static Font GetFont(string name, float size, FontStyle style = FontStyle.Regular)
    {
      var sp = new Span<byte>(StackPtr, 1024); var wp = sp;
      Write(ref wp, name); Write(ref wp, size); Write(ref wp, style);
      return (Font)Buffer.GetBuffer(typeof(Font), StackPtr, sp.Length - wp.Length, null);
    }

    internal struct Vertex { public Vector3 p, n; public Vector2 t; }

    public static void UpdateMesh(Vector3* pp, int np, ushort* ii, int ni, float smooth, ref IndexBuffer? ib, ref VertexBuffer? vb)
    {
      var kk = (int*)StackPtr; var tt = kk + ni;
      var e = 31 - BitOperations.LeadingZeroCount(unchecked((uint)ni));
      e = 1 << (e + 1); var w = e - 1; //if(e > 15) e = 15 // 64k limit?
      var dict = tt; for (int i = 0, n = e >> 1; i < n; i++) ((ulong*)dict)[i] = 0; Debug.Assert((e & 1) == 0 || ni == 0);
      for (int i = ni - 1, m = 0b010010; i >= 0; i--, m = (m >> 1) | ((m & 1) << 5))
      {
        int j = i - (m & 3), k = j + ((m >> 2) & 3), v = j + ((m >> 1) & 3), h;
        dict[e] = k = ii[i] | (ii[k] << 16); dict[e + 1] = v;
        dict[e + 2] = dict[h = (k ^ ((k >> 16) * 31)) & w]; dict[h] = e; e += 3;
      }
      for (int i = 0, m = 0b100100; i < ni; i++, m = (m << 1) & 0b111111 | (m >> 5))
      {
        int j = i - (m & 3), k = (ii[j + ((m >> 2) & 3)]) | (ii[i] << 16), h = (k ^ ((k >> 16) * 31)) & w, t;
        for (t = dict[h]; t != 0; t = dict[t + 2]) if (dict[t] == k) { dict[t] = -1; break; }
        kk[i] = ii[t != 0 ? dict[t + 1] : j + ((m >> 1) & 3)];
      }
      var vv = (Vertex*)tt;
      for (int i = 0; i < np; i++) { vv[i].p = pp[i]; vv[i].t = default; }
      /////////////////
      for (int i = 0; i < ni; i += 3)
      {
        var p1 = vv[ii[i + 0]].p;
        var p2 = vv[ii[i + 1]].p;
        var p3 = vv[ii[i + 2]].p; //if (p1 == p2 || p2 == p3 || p3 == p1) { }
        var cp = Vector3.Cross(p2 - p1, p3 - p1);
        var lc = cp.Length();  // vector.length(vector.ccw(p2 - p1, p3 - p1)); //if (lc < 0.0000001f) { }
        var no = cp * (1 / lc); // vector.normalize(vector.ccw(p2 - p1, p3 - p1));
        for (int k = 0; k < 3; k++)
        {
          int j = ii[i + k];
          for (; ; )
          {
            var c = *(int*)&vv[j].t.X; if (c == 0) { vv[j].n = *(Vector3*)&no; *(int*)&vv[j].t.X = 1; break; }
            var nt = vv[j].n;
            if (((c == 1 ? nt : Vector3.Normalize(nt)) - no).LengthSquared() < smooth) { vv[j].n = no + nt; *(int*)&vv[j].t.X = c + 1; break; }
            var l = *(int*)&vv[j].t.Y; if (l != 0) { j = l - 1; continue; }
            *(int*)&vv[j].t.Y = np + 1; vv[np].p = vv[j].p; vv[np].t = default; j = np++;
          }
          kk[i + k] = (kk[i + k] << 16) | j;
        }
      }
      for (int i = 0; i < np; i++)
      {
        vv[i].n = Vector3.Normalize(vv[i].n); //if (tex != null) { var p = Vector3.Transform(vv[i].p, *tex); vv[i].t = *(Vector2*)&p; } else 
        vv[i].t = default;
      }
      ib = (IndexBuffer)Buffer.GetBuffer(typeof(IndexBuffer), kk, ni * sizeof(int), ib);
      vb = (VertexBuffer)Buffer.GetBuffer(typeof(VertexBuffer), vv, np * sizeof(Vertex), vb);
    }

    static protected Span<T> stackspan<T>() where T : unmanaged
    {
      return new Span<T>(StackPtr, (maxstack - unchecked((int)(StackPtr - BasePtr))) / sizeof(T));
    }
    public static string ToString<T>(ReadOnlySpan<T> a, ReadOnlySpan<char> fmt = default) where T : ISpanFormattable
    {
      IFormatProvider prov = System.Globalization.NumberFormatInfo.InvariantInfo;
      Span<char> s = new Span<char>(StackPtr, maxstack >> 2), w = s;
      for (int i = 0; i < a.Length; i++)
      {
        if (i != 0) { w[0] = ' '; w = w.Slice(1); }
        a[i].TryFormat(w, out var c, fmt, prov); w = w.Slice(c);
      }
      return s.Slice(0, s.Length - w.Length).ToString();
    }

    public static void GlyphContour(char* ss, int ns, System.Drawing.Font font, int pixs, Action<int, int, int> act)
    {
      if (wdc == null) wdc = GetDC(null); var hfont = font.ToHfont().ToPointer();
      var po = SelectObject(wdc, hfont);
      var re = default(GCP_RESULTS); re.lStructSize = sizeof(GCP_RESULTS);
      re.lpDx = (int*)StackPtr; re.nGlyphs = ns; var pph = (byte*)(re.lpDx + ns);
      GetCharacterPlacementW(wdc, ss, ns, 0, &re, 0x0008); //GCP_USEKERNING
      int x = 0; Matrix3x2 gm, ma; var m2 = (int*)&ma; m2[0] = m2[3] = 0x10000; //MAT2
      for (int i = 0; i < ns; i++)
      {
        var c = ss[i]; if (c < ' ') { act(3, c, 0); if (c == '\n') x = 0; continue; }
        var ph = pph; //TTPOLYGONHEADER
        int nc = GetGlyphOutlineW(wdc, c, 2, &gm, maxstack >> 1, ph, m2); // GGO_NATIVE
        for (; ph - pph < nc; ph = ph + ((int*)ph)[0])
        {
          act(0, x, 0); //BeginContour at x
          act(1, ((int*)ph)[2], ((int*)ph)[3]); //AddVertex
          var pc = ph + 16; //TTPOLYCURVE
          for (; pc < ph + ((int*)ph)[0]; pc = pc + 4 + ((ushort*)pc)[1] * 8)
          {
            var cpfx = ((ushort*)pc)[1]; var pfx = ((int x, int y)*)(pc + 4);
            if (((ushort*)pc)[0] == 1) //TT_PRIM_LINE
            {
              for (int t = 0; t < cpfx; t++) act(1, pfx[t].x, pfx[t].y); //AddVertex                  
            }
            else //if (((ushort*)pc)[0] == 2) //TT_PRIM_QSPLINE
            {
              var spline = ((int x, int y)*)(&gm);//[3]
              spline[0] = *((int x, int y)*)(((byte*)pc) - 8);
              for (int k = 0; k < cpfx;)
              {
                spline[1] = pfx[k++];
                if (k == cpfx - 1) spline[2] = pfx[k++];
                else
                {
                  spline[2].x = (pfx[k - 1].x + pfx[k].x) >> 1;
                  spline[2].y = (pfx[k - 1].y + pfx[k].y) >> 1;
                }
                qspline(act, spline, 1 << pixs);
                spline[0] = spline[2];
              }
            }
          }
          act(2, 0, 0); // EndContour
        }
        x += re.lpDx[i];
      }
      SelectObject(wdc, po);
      DeleteObject(hfont);
      static void qspline(Action<int, int, int> act, (int x, int y)* qs, int pixs) //todo: qspline SSE
      {
        int ax = qs[0].x >> 10, ay = qs[0].y >> 10;
        int bx = qs[1].x >> 10, by = qs[1].y >> 10;
        int cx = qs[2].x >> 10, cy = qs[2].y >> 10;
        int gx = bx, dx, ddx = (dx = ax - gx) - gx + cx;
        int gy = by, dy, ddy = (dy = ay - gy) - gy + cy;
        gx = ddx < 0 ? -ddx : ddx;
        gy = ddy < 0 ? -ddy : ddy;
        gx += gx > gy ? gx + gy : gy + gy;
        for (gy = 1; gx > pixs; gx >>= 2) gy++;
        if (gy > 8) gy = 8;
        int i = 1 << gy;
        if (gy > 5)
        {
          ddx = gy - 1;
          qs[0].x = ax;
          qs[0].y = ay;
          qs[1].x = (ax + bx + 1) >> 1;
          qs[1].y = (ay + by + 1) >> 1;
          qs[2].x = (ax + bx + bx + cx + 2) >> 2;
          qs[2].y = (ay + by + by + cy + 2) >> 2; var t = qs[2];
          qspline(act, qs, pixs);
          qs[0] = t;
          qs[1].x = (cx + bx + 1) >> 1;
          qs[1].y = (cy + by + 1) >> 1;
          qs[2].x = cx;
          qs[2].y = cy;
          qspline(act, qs, pixs);
          return;
        }
        int sq = gy + gy;
        dx = ddx - (dx << ++gy); ddx += ddx;
        dy = ddy - (dy << gy); ddy += ddy;
        gy = ay << sq;
        gx = ax << sq;
        for (int t = 1 << (sq - 1); ;)
        {
          gx += dx; dx += ddx; gy += dy;
          act(1, ((gx + t) >> sq) << 10, ((gy + t) >> sq) << 10); //AddVertex
          if (--i == 0) break; dy += ddy;
        }
      }
    }

    static Dictionary<System.Reflection.PropertyInfo, object>? propdict;
    public record class PropAcc<T>(Func<object, T> get, Action<object, T> set);
    public static PropAcc<T> GetPropAcc<T>(System.Reflection.PropertyInfo pi)
    {
      Debug.Assert(pi.DeclaringType == pi.ReflectedType); // better: pi.DeclaringType.GetProperty(pi.Name) !!!
      if (!(propdict ??= new()).TryGetValue(pi, out var p))
      {
        var t0 = Expression.Parameter(typeof(object));
        var t2 = Expression.Property(Expression.Convert(t0, pi.DeclaringType), pi);
        var u2 = t2.Type != typeof(T) ? Expression.Convert(t2, typeof(T)) : (Expression)t2;
        var t3 = Expression.Lambda<Func<object, T>>(u2, pi.Name, new ParameterExpression[] { t0 });
        var t5 = Expression.Parameter(typeof(T));
        var t6 = Expression.Assign(t2, t2.Type != t5.Type ? Expression.Convert(t5, t2.Type) : t5);
        var t8 = Expression.Lambda<Action<object, T>>(t6, pi.Name, new ParameterExpression[] { t0, t5 });
        propdict.Add(pi, p = new PropAcc<T>(t3.Compile(), t8.Compile()));
      }
      return (PropAcc<T>)p;
    }

  }
  //shader
  unsafe partial class DX11Ctrl
  {
    //static void shaderdump()
    //{
    //  foreach (string f in Directory.GetFiles(@"C:\Users\cohle\Desktop\Speedy\SpeedyGraphics\obj", "*."))
    //    Debug.WriteLine("static readonly byte[] " + Path.GetFileName(f) + " = {" + string.Join(", ", File.ReadAllBytes(f).Select(p => p.ToString())) + "};");
    //}
    static readonly byte[] gs_line = { 68, 88, 66, 67, 248, 173, 121, 6, 105, 119, 93, 1, 63, 59, 37, 161, 123, 115, 29, 136, 1, 0, 0, 0, 156, 16, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 224, 1, 0, 0, 52, 2, 0, 0, 180, 2, 0, 0, 0, 16, 0, 0, 82, 68, 69, 70, 164, 1, 0, 0, 1, 0, 0, 0, 104, 0, 0, 0, 1, 0, 0, 0, 60, 0, 0, 0, 0, 5, 83, 71, 8, 1, 0, 0, 124, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 70, 114, 97, 109, 101, 0, 171, 92, 0, 0, 0, 3, 0, 0, 0, 128, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 20, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 56, 1, 0, 0, 64, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 76, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 112, 1, 0, 0, 80, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 76, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 86, 105, 101, 119, 80, 114, 111, 106, 101, 99, 116, 105, 111, 110, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 1, 0, 0, 103, 95, 102, 65, 109, 98, 105, 101, 110, 116, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 67, 1, 0, 0, 103, 95, 118, 76, 105, 103, 104, 116, 68, 105, 114, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 76, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 7, 3, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 3, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 79, 83, 71, 53, 120, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 104, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 8, 0, 0, 0, 0, 0, 0, 111, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 12, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 83, 72, 69, 88, 68, 13, 0, 0, 80, 0, 2, 0, 81, 3, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 0, 0, 0, 0, 4, 0, 0, 0, 95, 0, 0, 4, 114, 16, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 95, 0, 0, 4, 50, 16, 32, 0, 4, 0, 0, 0, 1, 0, 0, 0, 104, 0, 0, 2, 5, 0, 0, 0, 93, 48, 0, 1, 143, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 92, 40, 0, 1, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 114, 32, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 2, 0, 0, 0, 94, 0, 0, 2, 8, 0, 0, 0, 0, 0, 0, 10, 50, 0, 16, 0, 0, 0, 0, 0, 70, 16, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 16, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 7, 66, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 68, 0, 0, 5, 66, 0, 16, 0, 0, 0, 0, 0, 42, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 7, 50, 0, 16, 0, 0, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 10, 50, 0, 16, 0, 1, 0, 0, 0, 70, 16, 32, 128, 65, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 7, 66, 0, 16, 0, 1, 0, 0, 0, 70, 0, 16, 0, 1, 0, 0, 0, 70, 0, 16, 0, 1, 0, 0, 0, 68, 0, 0, 5, 66, 0, 16, 0, 1, 0, 0, 0, 42, 0, 16, 0, 1, 0, 0, 0, 56, 0, 0, 7, 50, 0, 16, 0, 1, 0, 0, 0, 166, 10, 16, 0, 1, 0, 0, 0, 70, 0, 16, 0, 1, 0, 0, 0, 0, 0, 0, 10, 50, 0, 16, 0, 2, 0, 0, 0, 70, 16, 32, 128, 65, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 16, 32, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 7, 66, 0, 16, 0, 2, 0, 0, 0, 70, 0, 16, 0, 2, 0, 0, 0, 70, 0, 16, 0, 2, 0, 0, 0, 68, 0, 0, 5, 66, 0, 16, 0, 2, 0, 0, 0, 42, 0, 16, 0, 2, 0, 0, 0, 56, 0, 0, 7, 50, 0, 16, 0, 2, 0, 0, 0, 166, 10, 16, 0, 2, 0, 0, 0, 70, 0, 16, 0, 2, 0, 0, 0, 54, 0, 0, 6, 194, 0, 16, 0, 0, 0, 0, 0, 86, 5, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 194, 0, 16, 0, 1, 0, 0, 0, 86, 5, 16, 128, 65, 0, 0, 0, 1, 0, 0, 0, 15, 0, 0, 7, 34, 0, 16, 0, 0, 0, 0, 0, 134, 0, 16, 0, 0, 0, 0, 0, 134, 0, 16, 0, 1, 0, 0, 0, 49, 0, 0, 7, 34, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 51, 51, 115, 63, 26, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 7, 82, 0, 16, 0, 0, 0, 0, 0, 246, 12, 16, 0, 0, 0, 0, 0, 246, 12, 16, 0, 1, 0, 0, 0, 15, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 134, 0, 16, 0, 0, 0, 0, 0, 134, 0, 16, 0, 0, 0, 0, 0, 68, 0, 0, 5, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 7, 82, 0, 16, 0, 0, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 6, 2, 16, 0, 0, 0, 0, 0, 56, 0, 0, 10, 194, 0, 16, 0, 1, 0, 0, 0, 86, 1, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 191, 0, 0, 128, 63, 55, 0, 0, 9, 50, 0, 16, 0, 0, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 134, 0, 16, 0, 0, 0, 0, 0, 230, 10, 16, 0, 1, 0, 0, 0, 50, 0, 0, 12, 194, 0, 16, 0, 0, 0, 0, 0, 6, 4, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 86, 21, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 6, 20, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 3, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 50, 0, 0, 11, 50, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 86, 21, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 70, 16, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 1, 64, 0, 0, 0, 0, 128, 63, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 6, 194, 0, 16, 0, 2, 0, 0, 0, 86, 5, 16, 128, 65, 0, 0, 0, 2, 0, 0, 0, 50, 0, 0, 12, 50, 0, 16, 0, 0, 0, 0, 0, 22, 5, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 128, 191, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 38, 10, 16, 0, 2, 0, 0, 0, 15, 0, 0, 7, 66, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 68, 0, 0, 5, 66, 0, 16, 0, 0, 0, 0, 0, 42, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 7, 50, 0, 16, 0, 0, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 15, 0, 0, 7, 66, 0, 16, 0, 0, 0, 0, 0, 182, 15, 16, 0, 1, 0, 0, 0, 198, 0, 16, 0, 2, 0, 0, 0, 49, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 51, 51, 115, 63, 42, 0, 16, 0, 0, 0, 0, 0, 55, 0, 0, 9, 194, 0, 16, 0, 2, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 6, 4, 16, 0, 0, 0, 0, 0, 166, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 12, 50, 0, 16, 0, 3, 0, 0, 0, 230, 10, 16, 128, 65, 0, 0, 0, 2, 0, 0, 0, 86, 21, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 70, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 4, 0, 0, 0, 86, 5, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 3, 0, 0, 0, 6, 0, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 50, 0, 0, 11, 194, 0, 16, 0, 2, 0, 0, 0, 166, 14, 16, 0, 2, 0, 0, 0, 86, 21, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 6, 20, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 246, 15, 16, 0, 2, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 3, 0, 0, 0, 166, 10, 16, 0, 2, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 1, 64, 0, 0, 0, 0, 128, 63, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 49, 0, 0, 7, 66, 0, 16, 0, 0, 0, 0, 0, 42, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 119, 190, 127, 191, 60, 0, 0, 7, 66, 0, 16, 0, 0, 0, 0, 0, 42, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 31, 0, 4, 3, 42, 0, 16, 0, 0, 0, 0, 0, 62, 0, 0, 1, 21, 0, 0, 1, 56, 0, 0, 8, 66, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 26, 0, 16, 128, 65, 0, 0, 0, 2, 0, 0, 0, 50, 0, 0, 11, 66, 0, 16, 0, 0, 0, 0, 0, 26, 0, 16, 128, 65, 0, 0, 0, 1, 0, 0, 0, 10, 0, 16, 0, 2, 0, 0, 0, 42, 0, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 49, 0, 0, 7, 66, 0, 16, 0, 0, 0, 0, 0, 42, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 54, 0, 0, 7, 18, 0, 16, 0, 3, 0, 0, 0, 26, 16, 32, 128, 65, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 6, 66, 0, 16, 0, 3, 0, 0, 0, 26, 16, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 8, 162, 0, 16, 0, 3, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 55, 0, 0, 9, 194, 0, 16, 0, 0, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 6, 4, 16, 0, 3, 0, 0, 0, 166, 14, 16, 0, 3, 0, 0, 0, 50, 0, 0, 11, 50, 0, 16, 0, 1, 0, 0, 0, 230, 10, 16, 128, 65, 0, 0, 0, 1, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 86, 5, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 6, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 63, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 50, 0, 0, 11, 50, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 56, 0, 0, 10, 50, 0, 16, 0, 0, 0, 0, 0, 22, 5, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 128, 191, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 50, 0, 16, 0, 0, 0, 0, 0, 70, 0, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 18, 32, 16, 0, 2, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 54, 0, 0, 5, 34, 32, 16, 0, 2, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 117, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 5, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] gs_outl3d = { 68, 88, 66, 67, 128, 95, 59, 209, 239, 152, 41, 73, 171, 252, 16, 137, 228, 33, 167, 37, 1, 0, 0, 0, 240, 16, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 224, 1, 0, 0, 20, 2, 0, 0, 148, 2, 0, 0, 84, 16, 0, 0, 82, 68, 69, 70, 164, 1, 0, 0, 1, 0, 0, 0, 104, 0, 0, 0, 1, 0, 0, 0, 60, 0, 0, 0, 0, 5, 83, 71, 8, 1, 0, 0, 124, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 70, 114, 97, 109, 101, 0, 171, 92, 0, 0, 0, 3, 0, 0, 0, 128, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 20, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 56, 1, 0, 0, 64, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 76, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 112, 1, 0, 0, 80, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 76, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 86, 105, 101, 119, 80, 114, 111, 106, 101, 99, 116, 105, 111, 110, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 1, 0, 0, 103, 95, 102, 65, 109, 98, 105, 101, 110, 116, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 67, 1, 0, 0, 103, 95, 118, 76, 105, 103, 104, 116, 68, 105, 114, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 7, 7, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 171, 171, 171, 79, 83, 71, 53, 120, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0, 104, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 8, 0, 0, 0, 0, 0, 0, 111, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 12, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 83, 72, 69, 88, 184, 13, 0, 0, 80, 0, 2, 0, 110, 3, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 0, 0, 0, 0, 6, 0, 0, 0, 95, 0, 0, 4, 114, 16, 32, 0, 6, 0, 0, 0, 0, 0, 0, 0, 104, 0, 0, 2, 4, 0, 0, 0, 93, 56, 0, 1, 143, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 92, 24, 0, 1, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 114, 32, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 2, 0, 0, 0, 94, 0, 0, 2, 6, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 38, 9, 16, 0, 0, 0, 0, 0, 150, 4, 16, 0, 1, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 70, 18, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 29, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 31, 0, 4, 3, 58, 0, 16, 0, 0, 0, 0, 0, 62, 0, 0, 1, 21, 0, 0, 1, 16, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 68, 0, 0, 5, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 0, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 2, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 3, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 38, 9, 16, 0, 1, 0, 0, 0, 150, 4, 16, 0, 2, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 2, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 70, 18, 32, 128, 65, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 2, 0, 0, 0, 29, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 49, 0, 0, 9, 130, 0, 16, 0, 1, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 10, 16, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 18, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 68, 0, 0, 5, 18, 0, 16, 0, 2, 0, 0, 0, 10, 0, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 63, 60, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 58, 0, 16, 0, 1, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 1, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 31, 0, 0, 3, 58, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 6, 16, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 166, 26, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 8, 50, 32, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 6, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 166, 26, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 8, 50, 32, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 21, 0, 0, 1, 0, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 2, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 3, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 38, 9, 16, 0, 1, 0, 0, 0, 150, 4, 16, 0, 2, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 2, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 70, 18, 32, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 2, 0, 0, 0, 29, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 49, 0, 0, 9, 130, 0, 16, 0, 1, 0, 0, 0, 10, 16, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 10, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 18, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 68, 0, 0, 5, 18, 0, 16, 0, 2, 0, 0, 0, 10, 0, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 63, 60, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 58, 0, 16, 0, 1, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 1, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 1, 0, 0, 0, 31, 0, 0, 3, 58, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 6, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 166, 26, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 8, 50, 32, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 6, 16, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 166, 26, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 8, 50, 32, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 21, 0, 0, 1, 0, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 2, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 3, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 38, 9, 16, 0, 1, 0, 0, 0, 150, 4, 16, 0, 2, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 2, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 70, 18, 32, 128, 65, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 2, 0, 0, 0, 29, 0, 0, 7, 130, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 49, 0, 0, 9, 130, 0, 16, 0, 1, 0, 0, 0, 10, 16, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 16, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 18, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 68, 0, 0, 5, 18, 0, 16, 0, 2, 0, 0, 0, 10, 0, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 63, 60, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 1, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 1, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 31, 0, 0, 3, 10, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 0, 0, 0, 0, 6, 16, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 0, 0, 0, 0, 166, 26, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 8, 50, 32, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 0, 0, 0, 0, 6, 16, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 0, 0, 0, 0, 166, 26, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 54, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 8, 50, 32, 16, 0, 2, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 21, 0, 0, 1, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 117, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 73, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 2, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 3, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] gs_shadows = { 68, 88, 66, 67, 160, 5, 194, 163, 78, 212, 120, 21, 32, 170, 46, 51, 7, 42, 42, 222, 1, 0, 0, 0, 132, 14, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 224, 1, 0, 0, 52, 2, 0, 0, 108, 2, 0, 0, 232, 13, 0, 0, 82, 68, 69, 70, 164, 1, 0, 0, 1, 0, 0, 0, 104, 0, 0, 0, 1, 0, 0, 0, 60, 0, 0, 0, 0, 5, 83, 71, 8, 1, 0, 0, 124, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 70, 114, 97, 109, 101, 0, 171, 92, 0, 0, 0, 3, 0, 0, 0, 128, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 20, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 56, 1, 0, 0, 64, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 76, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 112, 1, 0, 0, 80, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 76, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 86, 105, 101, 119, 80, 114, 111, 106, 101, 99, 116, 105, 111, 110, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 1, 0, 0, 103, 95, 102, 65, 109, 98, 105, 101, 110, 116, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 67, 1, 0, 0, 103, 95, 118, 76, 105, 103, 104, 116, 68, 105, 114, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 76, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 7, 7, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 79, 83, 71, 53, 48, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 36, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 83, 72, 69, 88, 116, 11, 0, 0, 80, 0, 2, 0, 221, 2, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 0, 0, 0, 0, 6, 0, 0, 0, 95, 0, 0, 4, 114, 16, 32, 0, 6, 0, 0, 0, 0, 0, 0, 0, 95, 0, 0, 4, 50, 16, 32, 0, 6, 0, 0, 0, 1, 0, 0, 0, 104, 0, 0, 2, 9, 0, 0, 0, 93, 56, 0, 1, 143, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 92, 40, 0, 1, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 94, 0, 0, 2, 18, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 1, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 2, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 38, 9, 16, 0, 0, 0, 0, 0, 150, 4, 16, 0, 1, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 2, 0, 0, 0, 16, 0, 0, 8, 18, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 29, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 149, 191, 214, 51, 10, 0, 16, 0, 0, 0, 0, 0, 31, 0, 4, 3, 10, 0, 16, 0, 0, 0, 0, 0, 62, 0, 0, 1, 21, 0, 0, 1, 14, 0, 0, 11, 18, 0, 16, 0, 0, 0, 0, 0, 2, 64, 0, 0, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 42, 128, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 6, 16, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 1, 0, 0, 0, 166, 26, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 10, 34, 0, 16, 0, 0, 0, 0, 0, 58, 128, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 42, 16, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 34, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 26, 0, 16, 0, 0, 0, 0, 0, 50, 0, 0, 11, 226, 0, 16, 0, 0, 0, 0, 0, 6, 137, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 6, 25, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 2, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 2, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 2, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 2, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 2, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 2, 0, 0, 0, 70, 14, 16, 0, 2, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 3, 0, 0, 0, 6, 16, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 3, 0, 0, 0, 166, 26, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 3, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 10, 34, 0, 16, 0, 0, 0, 0, 0, 58, 128, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 42, 16, 32, 128, 65, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 34, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 26, 0, 16, 0, 0, 0, 0, 0, 50, 0, 0, 11, 226, 0, 16, 0, 0, 0, 0, 0, 6, 137, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 6, 25, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 4, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 4, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 4, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 4, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 56, 0, 0, 9, 242, 0, 16, 0, 5, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 86, 21, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 5, 0, 0, 0, 6, 16, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 5, 0, 0, 0, 50, 0, 0, 11, 242, 0, 16, 0, 5, 0, 0, 0, 166, 26, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 5, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 5, 0, 0, 0, 70, 14, 16, 0, 5, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 10, 34, 0, 16, 0, 0, 0, 0, 0, 58, 128, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 42, 16, 32, 128, 65, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 26, 0, 16, 0, 0, 0, 0, 0, 50, 0, 0, 11, 114, 0, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 18, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 6, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 6, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 6, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 6, 0, 0, 0, 0, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 5, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 2, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 6, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 7, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 8, 0, 0, 0, 70, 2, 16, 0, 6, 0, 0, 0, 70, 2, 16, 0, 7, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 6, 0, 0, 0, 38, 9, 16, 0, 6, 0, 0, 0, 150, 4, 16, 0, 7, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 8, 0, 0, 0, 16, 0, 0, 8, 18, 0, 16, 0, 6, 0, 0, 0, 70, 2, 16, 0, 6, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 6, 0, 0, 0, 10, 0, 16, 0, 6, 0, 0, 0, 1, 64, 0, 0, 149, 191, 214, 51, 31, 0, 4, 3, 10, 0, 16, 0, 6, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 2, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 21, 0, 0, 1, 0, 0, 0, 10, 114, 0, 16, 0, 6, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 7, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 4, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 8, 0, 0, 0, 70, 2, 16, 0, 6, 0, 0, 0, 70, 2, 16, 0, 7, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 6, 0, 0, 0, 38, 9, 16, 0, 6, 0, 0, 0, 150, 4, 16, 0, 7, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 8, 0, 0, 0, 16, 0, 0, 8, 18, 0, 16, 0, 6, 0, 0, 0, 70, 2, 16, 0, 6, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 6, 0, 0, 0, 10, 0, 16, 0, 6, 0, 0, 0, 1, 64, 0, 0, 149, 191, 214, 51, 31, 0, 4, 3, 10, 0, 16, 0, 6, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 3, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 4, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 5, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 21, 0, 0, 1, 0, 0, 0, 10, 114, 0, 16, 0, 3, 0, 0, 0, 38, 25, 32, 128, 65, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 38, 25, 32, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 114, 0, 16, 0, 4, 0, 0, 0, 150, 20, 32, 128, 65, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 150, 20, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 0, 16, 0, 6, 0, 0, 0, 70, 2, 16, 0, 3, 0, 0, 0, 70, 2, 16, 0, 4, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 3, 0, 0, 0, 38, 9, 16, 0, 3, 0, 0, 0, 150, 4, 16, 0, 4, 0, 0, 0, 70, 2, 16, 128, 65, 0, 0, 0, 6, 0, 0, 0, 16, 0, 0, 8, 18, 0, 16, 0, 3, 0, 0, 0, 70, 2, 16, 0, 3, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 3, 0, 0, 0, 10, 0, 16, 0, 3, 0, 0, 0, 1, 64, 0, 0, 149, 191, 214, 51, 31, 0, 4, 3, 10, 0, 16, 0, 3, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 5, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 54, 0, 0, 5, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 2, 0, 0, 0, 117, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 118, 0, 0, 3, 0, 0, 17, 0, 0, 0, 0, 0, 21, 0, 0, 1, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 109, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 58, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 5, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] ps_font = { 68, 88, 66, 67, 242, 102, 192, 198, 138, 171, 144, 206, 88, 152, 172, 58, 147, 43, 128, 218, 1, 0, 0, 0, 212, 3, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 160, 1, 0, 0, 20, 2, 0, 0, 72, 2, 0, 0, 56, 3, 0, 0, 82, 68, 69, 70, 100, 1, 0, 0, 1, 0, 0, 0, 196, 0, 0, 0, 3, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 8, 1, 0, 0, 60, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 156, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 168, 0, 0, 0, 2, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 180, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 103, 95, 115, 97, 109, 76, 105, 110, 101, 97, 114, 0, 103, 95, 116, 120, 68, 105, 102, 102, 117, 115, 101, 0, 99, 98, 80, 115, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 171, 180, 0, 0, 0, 1, 0, 0, 0, 220, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 1, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 24, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 118, 68, 105, 102, 102, 117, 115, 101, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 1, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 0, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 65, 82, 71, 69, 84, 0, 171, 171, 83, 72, 69, 88, 232, 0, 0, 0, 80, 0, 0, 0, 58, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 90, 0, 0, 3, 0, 96, 16, 0, 0, 0, 0, 0, 88, 24, 0, 4, 0, 112, 16, 0, 0, 0, 0, 0, 85, 85, 0, 0, 98, 16, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 104, 0, 0, 2, 1, 0, 0, 0, 69, 0, 0, 139, 194, 0, 0, 128, 67, 85, 21, 0, 18, 0, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 54, 121, 16, 0, 0, 0, 0, 0, 0, 96, 16, 0, 0, 0, 0, 0, 24, 0, 0, 7, 34, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 13, 0, 4, 3, 26, 0, 16, 0, 0, 0, 0, 0, 56, 0, 0, 8, 130, 32, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 58, 128, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 114, 32, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 6, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] ps_main_0 = { 68, 88, 66, 67, 186, 87, 165, 254, 32, 86, 160, 77, 205, 230, 216, 190, 172, 143, 10, 241, 1, 0, 0, 0, 216, 2, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 72, 1, 0, 0, 188, 1, 0, 0, 240, 1, 0, 0, 60, 2, 0, 0, 82, 68, 69, 70, 12, 1, 0, 0, 1, 0, 0, 0, 108, 0, 0, 0, 1, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 8, 1, 0, 0, 228, 0, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 115, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 171, 92, 0, 0, 0, 1, 0, 0, 0, 132, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 172, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 192, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 118, 68, 105, 102, 102, 117, 115, 101, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 183, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 0, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 65, 82, 71, 69, 84, 0, 171, 171, 83, 72, 69, 88, 68, 0, 0, 0, 80, 0, 0, 0, 17, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 54, 0, 0, 6, 242, 32, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] ps_main_1 = { 68, 88, 66, 67, 156, 231, 47, 85, 1, 46, 226, 195, 111, 160, 204, 140, 205, 103, 10, 19, 1, 0, 0, 0, 188, 3, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 160, 1, 0, 0, 20, 2, 0, 0, 72, 2, 0, 0, 32, 3, 0, 0, 82, 68, 69, 70, 100, 1, 0, 0, 1, 0, 0, 0, 196, 0, 0, 0, 3, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 8, 1, 0, 0, 60, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 156, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 168, 0, 0, 0, 2, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 180, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 103, 95, 115, 97, 109, 76, 105, 110, 101, 97, 114, 0, 103, 95, 116, 120, 68, 105, 102, 102, 117, 115, 101, 0, 99, 98, 80, 115, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 171, 180, 0, 0, 0, 1, 0, 0, 0, 220, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 1, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 24, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 118, 68, 105, 102, 102, 117, 115, 101, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 1, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 7, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 65, 82, 71, 69, 84, 0, 171, 171, 83, 72, 69, 88, 208, 0, 0, 0, 80, 0, 0, 0, 52, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 90, 0, 0, 3, 0, 96, 16, 0, 0, 0, 0, 0, 88, 24, 0, 4, 0, 112, 16, 0, 0, 0, 0, 0, 85, 85, 0, 0, 98, 16, 0, 3, 114, 16, 16, 0, 1, 0, 0, 0, 98, 16, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 104, 0, 0, 2, 1, 0, 0, 0, 69, 0, 0, 139, 194, 0, 0, 128, 67, 85, 21, 0, 242, 0, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 70, 126, 16, 0, 0, 0, 0, 0, 0, 96, 16, 0, 0, 0, 0, 0, 56, 0, 0, 7, 114, 32, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 18, 16, 0, 1, 0, 0, 0, 56, 0, 0, 8, 130, 32, 16, 0, 0, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 58, 128, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 4, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] ps_main_2 = { 68, 88, 66, 67, 86, 162, 51, 134, 93, 235, 37, 57, 179, 15, 222, 209, 108, 54, 45, 92, 1, 0, 0, 0, 188, 3, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 160, 1, 0, 0, 20, 2, 0, 0, 72, 2, 0, 0, 32, 3, 0, 0, 82, 68, 69, 70, 100, 1, 0, 0, 1, 0, 0, 0, 196, 0, 0, 0, 3, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 8, 1, 0, 0, 60, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 156, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 168, 0, 0, 0, 2, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 180, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 103, 95, 115, 97, 109, 76, 105, 110, 101, 97, 114, 0, 103, 95, 116, 120, 68, 105, 102, 102, 117, 115, 101, 0, 99, 98, 80, 115, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 171, 180, 0, 0, 0, 1, 0, 0, 0, 220, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 1, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 24, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 118, 68, 105, 102, 102, 117, 115, 101, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 1, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 0, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 65, 82, 71, 69, 84, 0, 171, 171, 83, 72, 69, 88, 208, 0, 0, 0, 80, 0, 0, 0, 52, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 90, 0, 0, 3, 0, 96, 16, 0, 0, 0, 0, 0, 88, 24, 0, 4, 0, 112, 16, 0, 0, 0, 0, 0, 85, 85, 0, 0, 98, 16, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 104, 0, 0, 2, 2, 0, 0, 0, 69, 0, 0, 139, 194, 0, 0, 128, 67, 85, 21, 0, 242, 0, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 70, 126, 16, 0, 0, 0, 0, 0, 0, 96, 16, 0, 0, 0, 0, 0, 24, 0, 0, 7, 18, 0, 16, 0, 1, 0, 0, 0, 58, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 13, 0, 4, 3, 10, 0, 16, 0, 1, 0, 0, 0, 56, 0, 0, 8, 242, 32, 16, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 5, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] ps_main_3 = { 68, 88, 66, 67, 100, 118, 4, 97, 148, 190, 58, 121, 70, 239, 6, 238, 141, 43, 173, 143, 1, 0, 0, 0, 180, 3, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 160, 1, 0, 0, 20, 2, 0, 0, 72, 2, 0, 0, 24, 3, 0, 0, 82, 68, 69, 70, 100, 1, 0, 0, 1, 0, 0, 0, 196, 0, 0, 0, 3, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 8, 1, 0, 0, 60, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 156, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 168, 0, 0, 0, 2, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 180, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 103, 95, 115, 97, 109, 76, 105, 110, 101, 97, 114, 0, 103, 95, 116, 120, 68, 105, 102, 102, 117, 115, 101, 0, 99, 98, 80, 115, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 171, 180, 0, 0, 0, 1, 0, 0, 0, 220, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 1, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 24, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 118, 68, 105, 102, 102, 117, 115, 101, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 1, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 0, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 65, 82, 71, 69, 84, 0, 171, 171, 83, 72, 69, 88, 200, 0, 0, 0, 80, 0, 0, 0, 50, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 90, 0, 0, 3, 0, 96, 16, 0, 0, 0, 0, 0, 88, 24, 0, 4, 0, 112, 16, 0, 0, 0, 0, 0, 85, 85, 0, 0, 98, 16, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 104, 0, 0, 2, 1, 0, 0, 0, 69, 0, 0, 139, 194, 0, 0, 128, 67, 85, 21, 0, 18, 0, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 54, 121, 16, 0, 0, 0, 0, 0, 0, 96, 16, 0, 0, 0, 0, 0, 49, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 128, 63, 13, 0, 4, 3, 10, 0, 16, 0, 0, 0, 0, 0, 54, 0, 0, 6, 242, 32, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] ps_main_a = { 68, 88, 66, 67, 12, 140, 72, 93, 134, 122, 117, 177, 127, 92, 116, 49, 140, 225, 102, 20, 1, 0, 0, 0, 4, 3, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 72, 1, 0, 0, 188, 1, 0, 0, 240, 1, 0, 0, 104, 2, 0, 0, 82, 68, 69, 70, 12, 1, 0, 0, 1, 0, 0, 0, 108, 0, 0, 0, 1, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 8, 1, 0, 0, 228, 0, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 115, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 171, 92, 0, 0, 0, 1, 0, 0, 0, 132, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 172, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 192, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 118, 68, 105, 102, 102, 117, 115, 101, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 183, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 7, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 65, 82, 71, 69, 84, 0, 171, 171, 83, 72, 69, 88, 112, 0, 0, 0, 80, 0, 0, 0, 28, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 98, 16, 0, 3, 114, 16, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 56, 0, 0, 8, 114, 32, 16, 0, 0, 0, 0, 0, 70, 18, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 6, 130, 32, 16, 0, 0, 0, 0, 0, 58, 128, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] vs_main = { 68, 88, 66, 67, 70, 82, 12, 7, 212, 84, 7, 177, 165, 103, 133, 235, 157, 49, 140, 21, 1, 0, 0, 0, 120, 6, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 88, 2, 0, 0, 204, 2, 0, 0, 64, 3, 0, 0, 220, 5, 0, 0, 82, 68, 69, 70, 28, 2, 0, 0, 2, 0, 0, 0, 148, 0, 0, 0, 2, 0, 0, 0, 60, 0, 0, 0, 0, 5, 254, 255, 8, 1, 0, 0, 241, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 124, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 135, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 70, 114, 97, 109, 101, 0, 99, 98, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 124, 0, 0, 0, 3, 0, 0, 0, 196, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 135, 0, 0, 0, 1, 0, 0, 0, 192, 1, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 1, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 88, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 124, 1, 0, 0, 64, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 144, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 180, 1, 0, 0, 80, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 144, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 86, 105, 101, 119, 80, 114, 111, 106, 101, 99, 116, 105, 111, 110, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 78, 1, 0, 0, 103, 95, 102, 65, 109, 98, 105, 101, 110, 116, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 135, 1, 0, 0, 103, 95, 118, 76, 105, 103, 104, 116, 68, 105, 114, 0, 232, 1, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 88, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 87, 111, 114, 108, 100, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 171, 171, 171, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 15, 0, 0, 89, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 7, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 79, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 8, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 12, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 83, 72, 69, 88, 148, 2, 0, 0, 80, 0, 1, 0, 165, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 0, 0, 0, 0, 6, 0, 0, 0, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 4, 0, 0, 0, 95, 0, 0, 3, 242, 16, 16, 0, 0, 0, 0, 0, 95, 0, 0, 3, 114, 16, 16, 0, 1, 0, 0, 0, 95, 0, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 114, 32, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 2, 0, 0, 0, 104, 0, 0, 2, 2, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 246, 31, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 3, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 32, 16, 0, 0, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 56, 0, 0, 8, 114, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 2, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 16, 0, 0, 8, 18, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 52, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 4, 0, 0, 0, 54, 0, 0, 5, 50, 32, 16, 0, 2, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] vs_main_1 = { 68, 88, 66, 67, 104, 247, 50, 231, 118, 111, 249, 218, 129, 6, 219, 145, 16, 0, 70, 246, 1, 0, 0, 0, 192, 5, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 88, 2, 0, 0, 204, 2, 0, 0, 64, 3, 0, 0, 36, 5, 0, 0, 82, 68, 69, 70, 28, 2, 0, 0, 2, 0, 0, 0, 148, 0, 0, 0, 2, 0, 0, 0, 60, 0, 0, 0, 0, 5, 254, 255, 8, 1, 0, 0, 241, 1, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 124, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 135, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 70, 114, 97, 109, 101, 0, 99, 98, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 171, 124, 0, 0, 0, 3, 0, 0, 0, 196, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 135, 0, 0, 0, 1, 0, 0, 0, 192, 1, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 1, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 88, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 124, 1, 0, 0, 64, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 144, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 180, 1, 0, 0, 80, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 144, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 86, 105, 101, 119, 80, 114, 111, 106, 101, 99, 116, 105, 111, 110, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 78, 1, 0, 0, 103, 95, 102, 65, 109, 98, 105, 101, 110, 116, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 135, 1, 0, 0, 103, 95, 118, 76, 105, 103, 104, 116, 68, 105, 114, 0, 232, 1, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 88, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 87, 111, 114, 108, 100, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 171, 171, 171, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 15, 0, 0, 89, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 7, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 79, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 8, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 12, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 83, 72, 69, 88, 220, 1, 0, 0, 80, 0, 1, 0, 119, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 0, 0, 0, 0, 4, 0, 0, 0, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 4, 0, 0, 0, 95, 0, 0, 3, 242, 16, 16, 0, 0, 0, 0, 0, 95, 0, 0, 3, 114, 16, 16, 0, 1, 0, 0, 0, 95, 0, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 114, 32, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 2, 0, 0, 0, 104, 0, 0, 2, 2, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 246, 31, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 3, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 32, 16, 0, 0, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 54, 0, 0, 5, 114, 32, 16, 0, 1, 0, 0, 0, 70, 18, 16, 0, 1, 0, 0, 0, 54, 0, 0, 5, 50, 32, 16, 0, 2, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 11, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] vs_main_2 = { 68, 88, 66, 67, 170, 69, 103, 58, 209, 151, 220, 222, 76, 169, 30, 21, 100, 129, 61, 228, 1, 0, 0, 0, 116, 7, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 204, 2, 0, 0, 64, 3, 0, 0, 180, 3, 0, 0, 216, 6, 0, 0, 82, 68, 69, 70, 144, 2, 0, 0, 3, 0, 0, 0, 188, 0, 0, 0, 3, 0, 0, 0, 60, 0, 0, 0, 0, 5, 254, 255, 8, 1, 0, 0, 103, 2, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 156, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 167, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 179, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 70, 114, 97, 109, 101, 0, 99, 98, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 99, 98, 80, 101, 114, 84, 101, 120, 0, 156, 0, 0, 0, 3, 0, 0, 0, 4, 1, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 167, 0, 0, 0, 1, 0, 0, 0, 0, 2, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 179, 0, 0, 0, 1, 0, 0, 0, 52, 2, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 124, 1, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 152, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 188, 1, 0, 0, 64, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 208, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 244, 1, 0, 0, 80, 0, 0, 0, 16, 0, 0, 0, 2, 0, 0, 0, 208, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 86, 105, 101, 119, 80, 114, 111, 106, 101, 99, 116, 105, 111, 110, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 142, 1, 0, 0, 103, 95, 102, 65, 109, 98, 105, 101, 110, 116, 0, 102, 108, 111, 97, 116, 52, 0, 171, 171, 1, 0, 3, 0, 1, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 199, 1, 0, 0, 103, 95, 118, 76, 105, 103, 104, 116, 68, 105, 114, 0, 40, 2, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 152, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 87, 111, 114, 108, 100, 0, 171, 171, 171, 92, 2, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 152, 1, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 84, 101, 120, 116, 117, 114, 101, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 171, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 15, 0, 0, 89, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 7, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 79, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 8, 0, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 12, 0, 0, 83, 86, 95, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 83, 72, 69, 88, 28, 3, 0, 0, 80, 0, 1, 0, 199, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 0, 0, 0, 0, 6, 0, 0, 0, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 4, 0, 0, 0, 89, 0, 0, 4, 70, 142, 32, 0, 2, 0, 0, 0, 4, 0, 0, 0, 95, 0, 0, 3, 242, 16, 16, 0, 0, 0, 0, 0, 95, 0, 0, 3, 114, 16, 16, 0, 1, 0, 0, 0, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 114, 32, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 2, 0, 0, 0, 104, 0, 0, 2, 2, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 0, 0, 0, 0, 246, 31, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 1, 0, 0, 0, 3, 0, 0, 0, 70, 14, 16, 0, 0, 0, 0, 0, 56, 0, 0, 8, 242, 0, 16, 0, 1, 0, 0, 0, 86, 5, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 0, 16, 0, 1, 0, 0, 0, 166, 10, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 2, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 50, 0, 0, 10, 242, 32, 16, 0, 0, 0, 0, 0, 246, 15, 16, 0, 0, 0, 0, 0, 70, 142, 32, 0, 0, 0, 0, 0, 3, 0, 0, 0, 70, 14, 16, 0, 1, 0, 0, 0, 56, 0, 0, 8, 114, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 1, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 2, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 16, 0, 0, 8, 18, 0, 16, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 5, 0, 0, 0, 52, 0, 0, 7, 18, 0, 16, 0, 0, 0, 0, 0, 10, 0, 16, 0, 0, 0, 0, 0, 1, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 114, 32, 16, 0, 1, 0, 0, 0, 6, 0, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 0, 0, 0, 0, 4, 0, 0, 0, 56, 0, 0, 8, 50, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 0, 0, 0, 0, 70, 128, 32, 0, 2, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 50, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 0, 0, 0, 0, 70, 128, 32, 0, 2, 0, 0, 0, 0, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 50, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 0, 0, 0, 0, 70, 128, 32, 0, 2, 0, 0, 0, 2, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 50, 32, 16, 0, 2, 0, 0, 0, 246, 31, 16, 0, 0, 0, 0, 0, 70, 128, 32, 0, 2, 0, 0, 0, 3, 0, 0, 0, 70, 0, 16, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 19, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] vs_world = { 68, 88, 66, 67, 226, 113, 62, 54, 182, 217, 102, 171, 231, 33, 215, 108, 215, 58, 68, 148, 1, 0, 0, 0, 180, 3, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 68, 1, 0, 0, 184, 1, 0, 0, 12, 2, 0, 0, 24, 3, 0, 0, 82, 68, 69, 70, 8, 1, 0, 0, 1, 0, 0, 0, 104, 0, 0, 0, 1, 0, 0, 0, 60, 0, 0, 0, 0, 5, 254, 255, 8, 1, 0, 0, 224, 0, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 92, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 99, 98, 80, 101, 114, 79, 98, 106, 101, 99, 116, 0, 92, 0, 0, 0, 1, 0, 0, 0, 128, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 168, 0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 2, 0, 0, 0, 188, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 103, 95, 109, 87, 111, 114, 108, 100, 0, 102, 108, 111, 97, 116, 52, 120, 52, 0, 171, 171, 2, 0, 3, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 177, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 108, 0, 0, 0, 3, 0, 0, 0, 8, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 15, 0, 0, 89, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 7, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 3, 3, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 78, 79, 82, 77, 65, 76, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 79, 83, 71, 78, 76, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 7, 8, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 12, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 83, 72, 69, 88, 4, 1, 0, 0, 80, 0, 1, 0, 65, 0, 0, 0, 106, 8, 0, 1, 89, 0, 0, 4, 70, 142, 32, 0, 1, 0, 0, 0, 4, 0, 0, 0, 95, 0, 0, 3, 242, 16, 16, 0, 0, 0, 0, 0, 95, 0, 0, 3, 50, 16, 16, 0, 2, 0, 0, 0, 101, 0, 0, 3, 114, 32, 16, 0, 0, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 1, 0, 0, 0, 104, 0, 0, 2, 1, 0, 0, 0, 56, 0, 0, 8, 114, 0, 16, 0, 0, 0, 0, 0, 86, 21, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 1, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 6, 16, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 114, 0, 16, 0, 0, 0, 0, 0, 166, 26, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 2, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 50, 0, 0, 10, 114, 32, 16, 0, 0, 0, 0, 0, 246, 31, 16, 0, 0, 0, 0, 0, 70, 130, 32, 0, 1, 0, 0, 0, 3, 0, 0, 0, 70, 2, 16, 0, 0, 0, 0, 0, 54, 0, 0, 5, 50, 32, 16, 0, 1, 0, 0, 0, 70, 16, 16, 0, 2, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 6, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
  }
  //native
  unsafe partial class DX11Ctrl
  {
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    static extern int memcmp(void* a, void* b, nint n);
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    static extern void* memset(void* p, int v, nint n);
    [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    static extern void* memcpy(void* d, void* s, nint n);
    static void memcpy(byte[] d, void* s) { fixed (byte* p = d) memcpy(p, s, d.Length); }
    static void memcpy(void* d, byte[] s) { fixed (byte* p = s) memcpy(d, p, s.Length); }
    [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* VirtualAlloc(void* p, nint size, int type, int protect);
    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* SetTimer(IntPtr wnd, void* id, int dt, void* func);
    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* LoadCursor(void* hinst, void* s);
    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* SetCursor(void* p);
    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern bool ValidateRect(IntPtr wnd, int* rect);
    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* GetDC(void* wnd);
    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int ReleaseDC(void* wnd, void* dc);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* SelectObject(void* dc, void* p);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern bool DeleteObject(void* p);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* CreateFontW(int height, int width, int esc, int ori, int weight, int italic, int underline, int strike, int charset, int prec, int clip, int quality, int pitch, char* name);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int GetTextMetrics(void* dc, void* p);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int GetKerningPairsW(void* dc, int n, void* p);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int GetGlyphOutlineW(void* dc, int c, uint f, void* gm, int np, void* pp, void* mat);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern bool TextOutW(void* dc, int x, int y, char* s, int n);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern uint SetTextColor(void* dc, uint color);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int SetTextAlign(void* dc, int v);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int SetBkMode(void* dc, int v);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* CreateCompatibleDC(void* dc);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern bool DeleteDC(void* dc);
    [DllImport("gdi32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void* CreateDIBSection(void* dc, void* pbi, int usage, void* pp, void* sec, int offs);
    struct GCP_RESULTS
    {
      internal int lStructSize; //DWORD lStructSize;
      internal char* OutString; //LPWSTR lpOutString;
      internal uint* Order;     //UINT FAR *lpOrder;
      internal int* lpDx;       //int FAR  *lpDx;
      internal int* CaretPos;   //int FAR  *lpCaretPos;
      internal char* Class;     //LPSTR lpClass;
      internal char* Glyphs;    //LPWSTR lpGlyphs;
      internal int nGlyphs;     //UINT nGlyphs;
      internal int MaxFit;      //int nMaxFit;
    }
    [DllImport("gdi32.dll")]
    static extern uint GetCharacterPlacementW(void* hdc, char* s, int n, int maxext, GCP_RESULTS* p, uint flags);

    [DllImport("wininet.dll", CharSet = CharSet.Auto), SuppressUnmanagedCodeSecurity]
    static extern bool GetUrlCacheEntryInfo(string url, INTERNET_CACHE_ENTRY_INFO* p, ref int n);
    [StructLayout(LayoutKind.Sequential)]
    struct INTERNET_CACHE_ENTRY_INFO
    {
      internal uint size;
      internal char* url, path;
      //internal uint CacheEntryType; internal uint dwUseCount; internal uint dwHitRate; internal uint dwSizeLow; internal uint dwSizeHigh;
      //internal System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime; internal System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime; internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime; internal System.Runtime.InteropServices.ComTypes.FILETIME LastSyncTime;
      //internal void* lpHeaderInfo; internal uint dwHeaderInfoSize; internal char* lpszFileExtension; internal uint dwExemptDelta;
    };
    [DllImport("wininet.dll", CharSet = CharSet.Auto), SuppressUnmanagedCodeSecurity]
    static extern bool CreateUrlCacheEntry(
      [MarshalAs(UnmanagedType.LPTStr)] string url, uint filesize,
      [MarshalAs(UnmanagedType.LPTStr)] string ext,
      char* path, uint _);
    [DllImport("wininet.dll", CharSet = CharSet.Auto), SuppressUnmanagedCodeSecurity]
    static extern bool CommitUrlCacheEntry(
      [MarshalAs(UnmanagedType.LPTStr)] string url,
      [MarshalAs(UnmanagedType.LPTStr)] string path,
      System.Runtime.InteropServices.ComTypes.FILETIME exp,
      System.Runtime.InteropServices.ComTypes.FILETIME lmt, uint type,
      [MarshalAs(UnmanagedType.LPTStr)] string header, uint headersize,
      [MarshalAs(UnmanagedType.LPTStr)] string ext,
      [MarshalAs(UnmanagedType.LPTStr)] string? _);

    [DllImport("d3d11.dll"), SuppressUnmanagedCodeSecurity]
    static extern int D3D11CreateDevice(IAdapter? Adapter, D3D_DRIVER_TYPE DriverType, void* Software, CREATE_DEVICE_FLAG Flags, FEATURE_LEVEL* pFeatureLevels, int FeatureLevels, SDK_VERSION SDKVersion, out IDevice Device, FEATURE_LEVEL* Level, out IDeviceContext ImmediateContext);
    [DllImport("dxgi.dll"), SuppressUnmanagedCodeSecurity]
    static extern int CreateDXGIFactory([In, MarshalAs(UnmanagedType.LPStruct)] Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object unk);

    [ComImport, Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IDevice //: IDXGIDevice
    {
      void* CreateBuffer(BUFFER_DESC* Desc, SUBRESOURCE_DATA* pInitialData = null); //IBuffer
      void dummy2();
      //virtual HRESULT STDMETHODCALLTYPE CreateTexture1D( 
      //    /* [annotation] */
      //    __in  const D3D11_TEXTURE1D_DESC *pDesc,
      //    /* [annotation] */ 
      //    __in_xcount_opt(pDesc->MipLevels * pDesc->ArraySize)  const D3D11_SUBRESOURCE_DATA *pInitialData,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Texture1D **ppTexture1D) = 0;
      void* CreateTexture2D(TEXTURE2D_DESC* Desc, SUBRESOURCE_DATA* pInitialData = null); //ITexture2D
      void dummy4();
      //virtual HRESULT STDMETHODCALLTYPE CreateTexture3D( 
      //    /* [annotation] */ 
      //    __in  const D3D11_TEXTURE3D_DESC *pDesc,
      //    /* [annotation] */ 
      //    __in_xcount_opt(pDesc->MipLevels)  const D3D11_SUBRESOURCE_DATA *pInitialData,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Texture3D **ppTexture3D) = 0;
      void* CreateShaderResourceView(/*IResource*/void* Resource, SHADER_RESOURCE_VIEW_DESC* Desc); //IShaderResourceView
      void* CreateUnorderedAccessView(IResource pResource, UNORDERED_ACCESS_VIEW_DESC* Desc); //IUnorderedAccessView
      void* CreateRenderTargetView(/*IResource*/void* pResource, RENDER_TARGET_VIEW_DESC* Desc); //IRenderTargetView
      void* CreateDepthStencilView(/*IResource*/void* pResource, DEPTH_STENCIL_VIEW_DESC* Desc); //IDepthStencilView
      void* CreateInputLayout([MarshalAs(UnmanagedType.LPArray)] INPUT_ELEMENT_DESC[] pInputElementDescs, int NumElements, void* ShaderBytecodeWithInputSignature, UIntPtr BytecodeLength); //IInputLayout
      void* CreateVertexShader(void* pShaderBytecode, UIntPtr BytecodeLength, void*/*IClassLinkage*/ ClassLinkage = null); //IVertexShader
      void* CreateGeometryShader(void* pShaderBytecode, UIntPtr BytecodeLength, void*/*IClassLinkage*/ ClassLinkage = null); //IGeometryShader
      void dummy12();
      //virtual HRESULT STDMETHODCALLTYPE CreateGeometryShaderWithStreamOutput( 
      //    /* [annotation] */ 
      //    __in  const void *pShaderBytecode,
      //    /* [annotation] */ 
      //    __in  SIZE_T BytecodeLength,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumEntries)  const D3D11_SO_DECLARATION_ENTRY *pSODeclaration,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_SO_STREAM_COUNT * D3D11_SO_OUTPUT_COMPONENT_COUNT )  UINT NumEntries,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumStrides)  const UINT *pBufferStrides,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_SO_BUFFER_SLOT_COUNT )  UINT NumStrides,
      //    /* [annotation] */ 
      //    __in  UINT RasterizedStream,
      //    /* [annotation] */ 
      //    __in_opt  ID3D11ClassLinkage *pClassLinkage,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11GeometryShader **ppGeometryShader) = 0;
      void* CreatePixelShader(byte* ShaderBytecode, nint BytecodeLength, /*IClassLinkage*/void* ClassLinkage); //IPixelShader
      void dummy14();
      //virtual HRESULT STDMETHODCALLTYPE CreateHullShader( 
      //    /* [annotation] */ 
      //    __in  const void *pShaderBytecode,
      //    /* [annotation] */ 
      //    __in  SIZE_T BytecodeLength,
      //    /* [annotation] */ 
      //    __in_opt  ID3D11ClassLinkage *pClassLinkage,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11HullShader **ppHullShader) = 0;
      void dummy15();
      //virtual HRESULT STDMETHODCALLTYPE CreateDomainShader( 
      //    /* [annotation] */ 
      //    __in  const void *pShaderBytecode,
      //    /* [annotation] */ 
      //    __in  SIZE_T BytecodeLength,
      //    /* [annotation] */ 
      //    __in_opt  ID3D11ClassLinkage *pClassLinkage,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11DomainShader **ppDomainShader) = 0;
      void* CreateComputeShader(void* ShaderBytecode, nint BytecodeLength, /*IClassLinkage*/void* ClassLinkage = null); //IComputeShader
      void dummy17();
      //virtual HRESULT STDMETHODCALLTYPE CreateClassLinkage( 
      //    /* [annotation] */ 
      //    __out  ID3D11ClassLinkage **ppLinkage) = 0;
      void* CreateBlendState(BLEND_DESC* BlendStateDesc); //IBlendState
      void* CreateDepthStencilState(DEPTH_STENCIL_DESC* DepthStencilDesc); //IDepthStencilState
      void* CreateRasterizerState(RASTERIZER_DESC* RasterizerDesc); //IRasterizerState
      void* CreateSamplerState(SAMPLER_DESC* SamplerDesc); //ISamplerState
      void dummy22();
      //virtual HRESULT STDMETHODCALLTYPE CreateQuery( 
      //    /* [annotation] */ 
      //    __in  const D3D11_QUERY_DESC *pQueryDesc,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Query **ppQuery) = 0;
      void dummy23();
      //virtual HRESULT STDMETHODCALLTYPE CreatePredicate( 
      //    /* [annotation] */ 
      //    __in  const D3D11_QUERY_DESC *pPredicateDesc,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Predicate **ppPredicate) = 0;
      void dummy24();
      //virtual HRESULT STDMETHODCALLTYPE CreateCounter( 
      //    /* [annotation] */ 
      //    __in  const D3D11_COUNTER_DESC *pCounterDesc,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Counter **ppCounter) = 0;
      IDeviceContext CreateDeferredContext(int ContextFlags); //not SingleThreaded
      void dummy26();
      //virtual HRESULT STDMETHODCALLTYPE OpenSharedResource( 
      //    /* [annotation] */ 
      //    __in  HANDLE hResource,
      //    /* [annotation] */ 
      //    __in  REFIID ReturnedInterface,
      //    /* [annotation] */ 
      //    __out_opt  void **ppResource) = 0;
      FORMAT_SUPPORT CheckFormatSupport(FORMAT Format);
      [PreserveSig]
      int CheckMultisampleQualityLevels(FORMAT Format, int SampleCount, out int NumQualityLevels);
      void dummy29();
      //virtual void STDMETHODCALLTYPE CheckCounterInfo( 
      //    /* [annotation] */ 
      //    __out  D3D11_COUNTER_INFO *pCounterInfo) = 0;
      void dummy30();
      //virtual HRESULT STDMETHODCALLTYPE CheckCounter( 
      //    /* [annotation] */ 
      //    __in  const D3D11_COUNTER_DESC *pDesc,
      //    /* [annotation] */ 
      //    __out  D3D11_COUNTER_TYPE *pType,
      //    /* [annotation] */ 
      //    __out  UINT *pActiveCounters,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNameLength)  LPSTR szName,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNameLength,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pUnitsLength)  LPSTR szUnits,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pUnitsLength,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pDescriptionLength)  LPSTR szDescription,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pDescriptionLength) = 0;
      void dummy31();
      //virtual HRESULT STDMETHODCALLTYPE CheckFeatureSupport( 
      //    D3D11_FEATURE Feature,
      //    /* [annotation] */ 
      //    __out_bcount(FeatureSupportDataSize)  void *pFeatureSupportData,
      //    UINT FeatureSupportDataSize) = 0;
      void dummy32();
      //virtual HRESULT STDMETHODCALLTYPE GetPrivateData( 
      //    /* [annotation] */ 
      //    __in  REFGUID guid,
      //    /* [annotation] */ 
      //    __inout  UINT *pDataSize,
      //    /* [annotation] */ 
      //    __out_bcount_opt(*pDataSize)  void *pData) = 0;
      void dummy33();
      //virtual HRESULT STDMETHODCALLTYPE SetPrivateData( 
      //    /* [annotation] */ 
      //    __in  REFGUID guid,
      //    /* [annotation] */ 
      //    __in  UINT DataSize,
      //    /* [annotation] */ 
      //    __in_bcount_opt(DataSize)  const void *pData) = 0;
      void dummy34();
      //virtual HRESULT STDMETHODCALLTYPE SetPrivateDataInterface( 
      //    /* [annotation] */ 
      //    __in  REFGUID guid,
      //    /* [annotation] */ 
      //    __in_opt  const IUnknown *pData) = 0;
      FEATURE_LEVEL FeatureLevel { [PreserveSig] get; }
      uint CreationFlags { [PreserveSig] get; }
      [PreserveSig]
      int GetDeviceRemovedReason();
      void dummy38();
      //virtual void STDMETHODCALLTYPE GetImmediateContext( 
      //    /* [annotation] */ 
      //    __out  ID3D11DeviceContext **ppImmediateContext) = 0;
      //uint ExceptionMode { set; [PreserveSig] get; }
    }

    enum DEPTH_WRITE_MASK { ZERO = 0, ALL = 1 }
    enum COMPARISON { NEVER = 1, LESS = 2, EQUAL = 3, LESS_EQUAL = 4, GREATER = 5, NOT_EQUAL = 6, GREATER_EQUAL = 7, ALWAYS = 8 }
    enum STENCIL_OP { KEEP = 1, ZERO = 2, REPLACE = 3, INCR_SAT = 4, DECR_SAT = 5, INVERT = 6, INCR = 7, DECR = 8 }

    struct DEPTH_STENCILOP_DESC
    {
      public STENCIL_OP StencilFailOp;
      public STENCIL_OP StencilDepthFailOp;
      public STENCIL_OP StencilPassOp;
      public COMPARISON StencilFunc;
    }
    struct DEPTH_STENCIL_DESC
    {
      public int DepthEnable;
      public DEPTH_WRITE_MASK DepthWriteMask;
      public COMPARISON DepthFunc;
      public int StencilEnable;
      public byte StencilReadMask;
      public byte StencilWriteMask;
      public DEPTH_STENCILOP_DESC FrontFace;
      public DEPTH_STENCILOP_DESC BackFace;
    }
    struct SUBRESOURCE_DATA
    {
      public void* pSysMem;
      public int SysMemPitch;
      public int SysMemSlicePitch;
    }

    enum USAGE { DEFAULT = 0, IMMUTABLE = 1, DYNAMIC = 2, STAGING = 3 }
    enum BIND
    {
      VERTEX_BUFFER = 0x1,
      INDEX_BUFFER = 0x2,
      CONSTANT_BUFFER = 0x4,
      SHADER_RESOURCE = 0x8,
      STREAM_OUTPUT = 0x10,
      RENDER_TARGET = 0x20,
      DEPTH_STENCIL = 0x40,
      UNORDERED_ACCESS = 0x80
    }
    enum CPU_ACCESS_FLAG { WRITE = 0x10000, READ = 0x20000 }

    struct BUFFER_DESC
    {
      public int ByteWidth;
      public USAGE Usage;
      public BIND BindFlags;
      public CPU_ACCESS_FLAG CPUAccessFlags;
      public RESOURCE_MISC MiscFlags;
      public int StructureByteStride;
    }

    [ComImport, Guid("1841e5c8-16b0-489b-bcc8-44cfb0d5deae"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IDeviceChild
    {
      IDevice Device { get; }
      //virtual HRESULT STDMETHODCALLTYPE GetPrivateData( 
      //    /* [annotation] */ 
      //    __in  REFGUID guid,
      //    /* [annotation] */ 
      //    __inout  UINT *pDataSize,
      //    /* [annotation] */ 
      //    __out_bcount_opt( *pDataSize )  void *pData) = 0;
      //
      //virtual HRESULT STDMETHODCALLTYPE SetPrivateData( 
      //    /* [annotation] */ 
      //    __in  REFGUID guid,
      //    /* [annotation] */ 
      //    __in  UINT DataSize,
      //    /* [annotation] */ 
      //    __in_bcount_opt( DataSize )  const void *pData) = 0;
      //
      //virtual HRESULT STDMETHODCALLTYPE SetPrivateDataInterface( 
      //    /* [annotation] */ 
      //    __in  REFGUID guid,
      //    /* [annotation] */ 
      //    __in_opt  const IUnknown *pData) = 0;     
    }

    //[ComImport, Guid("3b301d64-d678-4289-8897-22f8928b72f3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IVertexShader : IDeviceChild { }
    //[ComImport, Guid("ea82e40d-51dc-4f33-93d4-db7c9125ae8c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IPixelShader : IDeviceChild { }
    //[ComImport, Guid("38325b96-effb-4022-ba02-2e795b70275c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IGeometryShader : IDeviceChild { }
    //[ComImport, Guid("4f5b196e-c2bd-495e-bd01-1fded38e4969"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IComputeShader : IDeviceChild { }
    //[ComImport, Guid("e4819ddc-4cf0-4025-bd26-5de82a3e07b7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IInputLayout : IDeviceChild { }//void _VtblGap1_4(); }
    //[ComImport, Guid("48570b85-d1ee-4fcd-a250-eb350722b037"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IBuffer : IResource { void _VtblGap1_7(); BUFFER_DESC Desc { get; } }
    //[ComImport, Guid("ddf57cba-9543-46e4-a12b-f207a0fe7fed"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IClassLinkage : IDeviceChild { void _VtblGap1_4(); }
    //[ComImport, Guid("75b68faa-347d-4159-8f45-a0640f01cd9a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IBlendState : IDeviceChild { void _VtblGap1_4(); BLEND_DESC Desc { get; } }
    //[ComImport, Guid("da6fea51-564c-4487-9810-f0d0f9b4e3a5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface ISamplerState : IDeviceChild { void _VtblGap1_4(); SAMPLER_DESC Desc { get; } } 
    //[ComImport, Guid("03823efb-8d8f-4e1c-9aa2-f64bb2cbfdf1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IDepthStencilState : IDeviceChild { void _VtblGap1_4(); DEPTH_STENCIL_DESC Desc { get; } }
    //[ComImport, Guid("9bb4ab81-ab1a-4d8f-b506-fc04200b6ee7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IRasterizerState : IDeviceChild { void _VtblGap1_4(); RASTERIZER_DESC Desc { get; } }
    //[ComImport, Guid("b0e06fe0-8192-4e1a-b1ca-36d7414710b2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IShaderResourceView : IView { void _VtblGap1_5(); SHADER_RESOURCE_VIEW_DESC Desc { get; } }
    //[ComImport, Guid("dfdba067-0b8d-4865-875b-d7b4516cc164"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IRenderTargetView : IView { void _VtblGap1_5(); RENDER_TARGET_VIEW_DESC Desc { get; } }
    //[ComImport, Guid("9fdac92a-1876-48c3-afad-25b94f84a9b6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IDepthStencilView : IView { void _VtblGap1_5(); DEPTH_STENCIL_VIEW_DESC Desc { get; } }
    //[ComImport, Guid("28acf509-7f5c-48f6-8611-f316010a6380"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    //public interface IUnorderedAccessView : IView { };

    enum INPUT_CLASSIFICATION
    {
      PER_VERTEX_DATA = 0,
      PER_INSTANCE_DATA = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT_ELEMENT_DESC
    {
      [MarshalAs(UnmanagedType.LPStr)]
      public string SemanticName;
      public int SemanticIndex;
      public FORMAT Format;
      public int InputSlot;
      public int AlignedByteOffset;
      public INPUT_CLASSIFICATION InputSlotClass;
      public int InstanceDataStepRate;
    }

    struct VIEWPORT
    {
      public float TopLeftX, TopLeftY, Width, Height, MinDepth, MaxDepth;
    }

    [ComImport, Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IDeviceContext : IDeviceChild
    {
      void _VtblGap1_4();
      [PreserveSig]
      void VSSetConstantBuffers(int StartSlot, int NumBuffers, [In] void** buffer); //IBuffer
      [PreserveSig]
      void PSSetShaderResources(int StartSlot, int NumViews, [In] void** ShaderResourceViews); //IShaderResourceView
      [PreserveSig]
      void PSSetShader(/*IPixelShader*/void* PixelShader, void* ppClassInstances = null, int NumClassInstances = 0);
      [PreserveSig]
      void PSSetSamplers(int StartSlot, int NumSamplers, [In] /*ISamplerState*/void** Samplers);
      [PreserveSig]
      void VSSetShader(/*IVertexShader*/void* VertexShader, void** ppClassInstances = null, int NumClassInstances = 0);
      [PreserveSig]
      void DrawIndexed(int IndexCount, int StartIndexLocation, int BaseVertexLocation);
      [PreserveSig]
      void Draw(int VertexCount, int StartVertex);
      MAPPED_SUBRESOURCE Map(/*IResource*/void* Resource, int Subresource, MAP MapType, int MapFlags);
      [PreserveSig]
      void Unmap(/*IResource*/void* Resource, int Subresource);
      [PreserveSig]
      void PSSetConstantBuffers(int StartSlot, int NumBuffers, [In] void** buffer); //IBuffer
      [PreserveSig]
      void IASetInputLayout(void* pInputLayout); //IInputLayout
      [PreserveSig]
      void IASetVertexBuffers(int StartSlot, int NumBuffers, [In] /*IBuffer*/void** VertexBuffers, [In] int* Strides, [In] int* Offsets);
      [PreserveSig]
      void IASetIndexBuffer(/*IBuffer*/void* IndexBuffer, FORMAT Format, int Offset);
      void dummy14();
      //virtual void STDMETHODCALLTYPE DrawIndexedInstanced( 
      //    /* [annotation] */ 
      //    __in  UINT IndexCountPerInstance,
      //    /* [annotation] */ 
      //    __in  UINT InstanceCount,
      //    /* [annotation] */ 
      //    __in  UINT StartIndexLocation,
      //    /* [annotation] */ 
      //    __in  INT BaseVertexLocation,
      //    /* [annotation] */ 
      //    __in  UINT StartInstanceLocation) = 0;
      void dummy15();
      //virtual void STDMETHODCALLTYPE DrawInstanced( 
      //    /* [annotation] */ 
      //    __in  UINT VertexCountPerInstance,
      //    /* [annotation] */ 
      //    __in  UINT InstanceCount,
      //    /* [annotation] */ 
      //    __in  UINT StartVertex,
      //    /* [annotation] */ 
      //    __in  UINT StartInstanceLocation) = 0;
      [PreserveSig]
      void GSSetConstantBuffers(int StartSlot, int NumBuffers, [In] void** buffer); //IBuffer
      [PreserveSig]
      void GSSetShader(/*IGeometryShader*/void* Shader, void** ppClassInstances = null, int NumClassInstances = 0);
      [PreserveSig]
      void IASetPrimitiveTopology(PRIMITIVE_TOPOLOGY Topology);
      [PreserveSig]
      void VSSetShaderResources(int StartSlot, int NumViews, [In] /*IShaderResourceView*/void** ShaderResourceViews);
      void dummy20();
      //virtual void STDMETHODCALLTYPE VSSetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __in_ecount(NumSamplers)  ID3D11SamplerState *const *ppSamplers) = 0;
      void dummy21();
      //virtual void STDMETHODCALLTYPE Begin( 
      //    /* [annotation] */ 
      //    __in  ID3D11Asynchronous *pAsync) = 0;
      void dummy22();
      //virtual void STDMETHODCALLTYPE End( 
      //    /* [annotation] */ 
      //    __in  ID3D11Asynchronous *pAsync) = 0;
      void dummy23();
      //virtual HRESULT STDMETHODCALLTYPE GetData( 
      //    /* [annotation] */ 
      //    __in  ID3D11Asynchronous *pAsync,
      //    /* [annotation] */ 
      //    __out_bcount_opt( DataSize )  void *pData,
      //    /* [annotation] */ 
      //    __in  UINT DataSize,
      //    /* [annotation] */ 
      //    __in  UINT GetDataFlags) = 0;
      void dummy24();
      //virtual void STDMETHODCALLTYPE SetPredication( 
      //    /* [annotation] */ 
      //    __in_opt  ID3D11Predicate *pPredicate,
      //    /* [annotation] */ 
      //    __in  BOOL PredicateValue) = 0;
      [PreserveSig]
      void GSSetShaderResources(int StartSlot, int NumViews, [In] /*IShaderResourceView*/void** ShaderResourceViews);
      void dummy26();
      //virtual void STDMETHODCALLTYPE GSSetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __in_ecount(NumSamplers)  ID3D11SamplerState *const *ppSamplers) = 0;
      [PreserveSig]
      void OMSetRenderTargets(int NumViews, [In] void**/*IRenderTargetView*/ RenderTargetViews, void*/*IDepthStencilView*/ DepthStencilView);
      void dummy28();
      //virtual void STDMETHODCALLTYPE OMSetRenderTargetsAndUnorderedAccessViews( 
      //    /* [annotation] */ 
      //    __in  UINT NumRTVs,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumRTVs)  ID3D11RenderTargetView *const *ppRenderTargetViews,
      //    /* [annotation] */ 
      //    __in_opt  ID3D11DepthStencilView *pDepthStencilView,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_PS_CS_UAV_REGISTER_COUNT - 1 )  UINT UAVStartSlot,
      //    /* [annotation] */ 
      //    __in  UINT NumUAVs,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumUAVs)  ID3D11UnorderedAccessView *const *ppUnorderedAccessViews,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumUAVs)  const UINT *pUAVInitialCounts) = 0;
      [PreserveSig]
      void OMSetBlendState(/*IBlendState*/void* BlendState, [In] in Vector4 BlendFactors, uint SampleMask);
      [PreserveSig]
      void OMSetDepthStencilState(/*IDepthStencilState*/void* DepthStencilState, int StencilRef);
      void dummy31();
      //virtual void STDMETHODCALLTYPE SOSetTargets( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_SO_BUFFER_SLOT_COUNT)  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumBuffers)  ID3D11Buffer *const *ppSOTargets,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumBuffers)  const UINT *pOffsets) = 0;
      [PreserveSig]
      void DrawAuto();
      void dummy33();
      //virtual void STDMETHODCALLTYPE DrawIndexedInstancedIndirect( 
      //    /* [annotation] */ 
      //    __in  ID3D11Buffer *pBufferForArgs,
      //    /* [annotation] */ 
      //    __in  UINT AlignedByteOffsetForArgs) = 0;
      void dummy34();
      //virtual void STDMETHODCALLTYPE DrawInstancedIndirect( 
      //    /* [annotation] */ 
      //    __in  ID3D11Buffer *pBufferForArgs,
      //    /* [annotation] */ 
      //    __in  UINT AlignedByteOffsetForArgs) = 0;
      [PreserveSig]
      void Dispatch(int x, int y, int z);
      [PreserveSig]
      void DispatchIndirect(/*IBuffer*/void* pBufferForArgs, int AlignedByteOffsetForArgs);
      [PreserveSig]
      void RSSetState(void* RasterizerState); //IRasterizerState
      [PreserveSig]
      void RSSetViewports(int NumViewports, VIEWPORT* Viewports);
      [PreserveSig]
      void RSSetScissorRects(int NumRects, RECT* pRects);
      [PreserveSig]
      void CopySubresourceRegion(IResource dst, int DstSubresource, int DstX, int DstY, int DstZ, IResource Src, int SrcSubresource, BOX* pSrcBox); //pSrcBox opt
      [PreserveSig]
      void CopyResource(/*IResource*/void* dst, /*IResource*/void* src);
      [PreserveSig]
      void UpdateSubresource(/*IResource*/void* DstResource, int DstSubresource, BOX* DstBox, void* pSrcData, int SrcRowPitch, int SrcDepthPitch);
      void dummy43();
      //virtual void STDMETHODCALLTYPE CopyStructureCount( 
      //    /* [annotation] */ 
      //    __in  ID3D11Buffer *pDstBuffer,
      //    /* [annotation] */ 
      //    __in  UINT DstAlignedByteOffset,
      //    /* [annotation] */ 
      //    __in  ID3D11UnorderedAccessView *pSrcView) = 0;
      [PreserveSig]
      void ClearRenderTargetView(void*/*IRenderTargetView*/ rtv, Vector4* rgba);
      void dummy45();
      //virtual void STDMETHODCALLTYPE ClearUnorderedAccessViewUint( 
      //    /* [annotation] */ 
      //    __in  ID3D11UnorderedAccessView *pUnorderedAccessView,
      //    /* [annotation] */ 
      //    __in  const UINT Values[ 4 ]) = 0;
      void dummy46();
      //virtual void STDMETHODCALLTYPE ClearUnorderedAccessViewFloat( 
      //    /* [annotation] */ 
      //    __in  ID3D11UnorderedAccessView *pUnorderedAccessView,
      //    /* [annotation] */ 
      //    __in  const FLOAT Values[ 4 ]) = 0;
      [PreserveSig]
      void ClearDepthStencilView(void*/*IDepthStencilView*/ DepthStencilView, CLEAR ClearFlags, float Depth, byte Stencil);
      [PreserveSig]
      void GenerateMips(void* srv); //IShaderResourceView
      void dummy49();
      //virtual void STDMETHODCALLTYPE SetResourceMinLOD( 
      //    /* [annotation] */ 
      //    __in  ID3D11Resource *pResource,
      //    FLOAT MinLOD) = 0;
      void dummy50();
      //virtual FLOAT STDMETHODCALLTYPE GetResourceMinLOD( 
      //    /* [annotation] */ 
      //    __in  ID3D11Resource *pResource) = 0;
      [PreserveSig]
      void ResolveSubresource(void* /*ID3D11Resource*/ dst, int dstsubres, void* /*ID3D11Resource*/ src, int srcsubres, FORMAT Format);
      void dummy52();
      //virtual void STDMETHODCALLTYPE ExecuteCommandList( 
      //    /* [annotation] */ 
      //    __in  ID3D11CommandList *pCommandList,
      //    BOOL RestoreContextState) = 0;
      [PreserveSig]
      void HSSetShaderResources(int StartSlot, int NumViews, [In] /*IShaderResourceView*/void** ShaderResourceViews);
      void dummy54();
      //virtual void STDMETHODCALLTYPE HSSetShader( 
      //    /* [annotation] */ 
      //    __in_opt  ID3D11HullShader *pHullShader,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumClassInstances)  ID3D11ClassInstance *const *ppClassInstances,
      //    UINT NumClassInstances) = 0;
      void dummy55();
      //virtual void STDMETHODCALLTYPE HSSetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __in_ecount(NumSamplers)  ID3D11SamplerState *const *ppSamplers) = 0;
      void dummy56();
      //virtual void STDMETHODCALLTYPE HSSetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __in_ecount(NumBuffers)  ID3D11Buffer *const *ppConstantBuffers) = 0;
      [PreserveSig]
      void DSSetShaderResources(int StartSlot, int NumViews, [In] /*IShaderResourceView*/void** ShaderResourceViews);
      void dummy58();
      //virtual void STDMETHODCALLTYPE DSSetShader( 
      //    /* [annotation] */ 
      //    __in_opt  ID3D11DomainShader *pDomainShader,
      //    /* [annotation] */ 
      //    __in_ecount_opt(NumClassInstances)  ID3D11ClassInstance *const *ppClassInstances,
      //    UINT NumClassInstances) = 0;
      void dummy59();
      //virtual void STDMETHODCALLTYPE DSSetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __in_ecount(NumSamplers)  ID3D11SamplerState *const *ppSamplers) = 0;
      void dummy60();
      //virtual void STDMETHODCALLTYPE DSSetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __in_ecount(NumBuffers)  ID3D11Buffer *const *ppConstantBuffers) = 0;
      [PreserveSig]
      void CSSetShaderResources(int StartSlot, int NumViews, [In] /*IShaderResourceView*/ void** ShaderResourceViews);
      [PreserveSig]
      void CSSetUnorderedAccessViews(int StartSlot, int NumUAVs, [In] /*IUnorderedAccessView*/ void** ppUnorderedAccessViews, int* pUAVInitialCounts);
      [PreserveSig]
      void CSSetShader(/*IComputeShader*/void* cs, void* ppClassInstances, int numClassInstances);
      void dummy64();
      //virtual void STDMETHODCALLTYPE CSSetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __in_ecount(NumSamplers)  ID3D11SamplerState *const *ppSamplers) = 0;
      [PreserveSig]
      void CSSetConstantBuffers(int StartSlot, int NumBuffers, [In] /*IBuffer*/void** ppConstantBuffers);
      void dummy66();
      //virtual void STDMETHODCALLTYPE VSGetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppConstantBuffers) = 0;
      void dummy67();
      //virtual void STDMETHODCALLTYPE PSGetShaderResources( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount(NumViews)  ID3D11ShaderResourceView **ppShaderResourceViews) = 0;
      void dummy68();
      //virtual void STDMETHODCALLTYPE PSGetShader( 
      //    /* [annotation] */ 
      //    __out  ID3D11PixelShader **ppPixelShader,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumClassInstances)  ID3D11ClassInstance **ppClassInstances,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNumClassInstances) = 0;
      void dummy69();
      //virtual void STDMETHODCALLTYPE PSGetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __out_ecount(NumSamplers)  ID3D11SamplerState **ppSamplers) = 0;
      void dummy70();
      //virtual void STDMETHODCALLTYPE VSGetShader( 
      //    /* [annotation] */ 
      //    __out  ID3D11VertexShader **ppVertexShader,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumClassInstances)  ID3D11ClassInstance **ppClassInstances,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNumClassInstances) = 0;
      void dummy71();
      //virtual void STDMETHODCALLTYPE PSGetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppConstantBuffers) = 0;
      void dummy72();
      //virtual void STDMETHODCALLTYPE IAGetInputLayout( 
      //    /* [annotation] */ 
      //    __out  ID3D11InputLayout **ppInputLayout) = 0;
      void dummy73();
      //virtual void STDMETHODCALLTYPE IAGetVertexBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount_opt(NumBuffers)  ID3D11Buffer **ppVertexBuffers,
      //    /* [annotation] */ 
      //    __out_ecount_opt(NumBuffers)  UINT *pStrides,
      //    /* [annotation] */ 
      //    __out_ecount_opt(NumBuffers)  UINT *pOffsets) = 0;
      void dummy74();
      //virtual void STDMETHODCALLTYPE IAGetIndexBuffer( 
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Buffer **pIndexBuffer,
      //    /* [annotation] */ 
      //    __out_opt  DXGI_FORMAT *Format,
      //    /* [annotation] */ 
      //    __out_opt  UINT *Offset) = 0;
      //
      //virtual void STDMETHODCALLTYPE GSGetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppConstantBuffers) = 0;
      //
      //virtual void STDMETHODCALLTYPE GSGetShader( 
      //    /* [annotation] */ 
      //    __out  ID3D11GeometryShader **ppGeometryShader,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumClassInstances)  ID3D11ClassInstance **ppClassInstances,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNumClassInstances) = 0;
      //
      //virtual void STDMETHODCALLTYPE IAGetPrimitiveTopology( 
      //    /* [annotation] */ 
      //    __out  D3D11_PRIMITIVE_TOPOLOGY *pTopology) = 0;
      //
      //virtual void STDMETHODCALLTYPE VSGetShaderResources( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount(NumViews)  ID3D11ShaderResourceView **ppShaderResourceViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE VSGetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __out_ecount(NumSamplers)  ID3D11SamplerState **ppSamplers) = 0;
      //
      //virtual void STDMETHODCALLTYPE GetPredication( 
      //    /* [annotation] */ 
      //    __out_opt  ID3D11Predicate **ppPredicate,
      //    /* [annotation] */ 
      //    __out_opt  BOOL *pPredicateValue) = 0;
      //
      //virtual void STDMETHODCALLTYPE GSGetShaderResources( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount(NumViews)  ID3D11ShaderResourceView **ppShaderResourceViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE GSGetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __out_ecount(NumSamplers)  ID3D11SamplerState **ppSamplers) = 0;
      //
      //virtual void STDMETHODCALLTYPE OMGetRenderTargets( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_SIMULTANEOUS_RENDER_TARGET_COUNT )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount_opt(NumViews)  ID3D11RenderTargetView **ppRenderTargetViews,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11DepthStencilView **ppDepthStencilView) = 0;
      //
      //virtual void STDMETHODCALLTYPE OMGetRenderTargetsAndUnorderedAccessViews( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_SIMULTANEOUS_RENDER_TARGET_COUNT )  UINT NumRTVs,
      //    /* [annotation] */ 
      //    __out_ecount_opt(NumRTVs)  ID3D11RenderTargetView **ppRenderTargetViews,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11DepthStencilView **ppDepthStencilView,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_PS_CS_UAV_REGISTER_COUNT - 1 )  UINT UAVStartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_PS_CS_UAV_REGISTER_COUNT - UAVStartSlot )  UINT NumUAVs,
      //    /* [annotation] */ 
      //    __out_ecount_opt(NumUAVs)  ID3D11UnorderedAccessView **ppUnorderedAccessViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE OMGetBlendState( 
      //    /* [annotation] */ 
      //    __out_opt  ID3D11BlendState **ppBlendState,
      //    /* [annotation] */ 
      //    __out_opt  FLOAT BlendFactor[ 4 ],
      //    /* [annotation] */ 
      //    __out_opt  UINT *pSampleMask) = 0;
      //
      //virtual void STDMETHODCALLTYPE OMGetDepthStencilState( 
      //    /* [annotation] */ 
      //    __out_opt  ID3D11DepthStencilState **ppDepthStencilState,
      //    /* [annotation] */ 
      //    __out_opt  UINT *pStencilRef) = 0;
      //
      //virtual void STDMETHODCALLTYPE SOGetTargets( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_SO_BUFFER_SLOT_COUNT )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppSOTargets) = 0;
      //
      //virtual void STDMETHODCALLTYPE RSGetState( 
      //    /* [annotation] */ 
      //    __out  ID3D11RasterizerState **ppRasterizerState) = 0;
      //
      //virtual void STDMETHODCALLTYPE RSGetViewports( 
      //    /* [annotation] */ 
      //    __inout /*_range(0, D3D11_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE )*/   UINT *pNumViewports,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumViewports)  D3D11_VIEWPORT *pViewports) = 0;
      //
      //virtual void STDMETHODCALLTYPE RSGetScissorRects( 
      //    /* [annotation] */ 
      //    __inout /*_range(0, D3D11_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE )*/   UINT *pNumRects,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumRects)  D3D11_RECT *pRects) = 0;
      //
      //virtual void STDMETHODCALLTYPE HSGetShaderResources( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount(NumViews)  ID3D11ShaderResourceView **ppShaderResourceViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE HSGetShader( 
      //    /* [annotation] */ 
      //    __out  ID3D11HullShader **ppHullShader,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumClassInstances)  ID3D11ClassInstance **ppClassInstances,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNumClassInstances) = 0;
      //
      //virtual void STDMETHODCALLTYPE HSGetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __out_ecount(NumSamplers)  ID3D11SamplerState **ppSamplers) = 0;
      //
      //virtual void STDMETHODCALLTYPE HSGetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppConstantBuffers) = 0;
      //
      //virtual void STDMETHODCALLTYPE DSGetShaderResources( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount(NumViews)  ID3D11ShaderResourceView **ppShaderResourceViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE DSGetShader( 
      //    /* [annotation] */ 
      //    __out  ID3D11DomainShader **ppDomainShader,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumClassInstances)  ID3D11ClassInstance **ppClassInstances,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNumClassInstances) = 0;
      //
      //virtual void STDMETHODCALLTYPE DSGetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __out_ecount(NumSamplers)  ID3D11SamplerState **ppSamplers) = 0;
      //
      //virtual void STDMETHODCALLTYPE DSGetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppConstantBuffers) = 0;
      //
      //virtual void STDMETHODCALLTYPE CSGetShaderResources( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT - StartSlot )  UINT NumViews,
      //    /* [annotation] */ 
      //    __out_ecount(NumViews)  ID3D11ShaderResourceView **ppShaderResourceViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE CSGetUnorderedAccessViews( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_PS_CS_UAV_REGISTER_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_PS_CS_UAV_REGISTER_COUNT - StartSlot )  UINT NumUAVs,
      //    /* [annotation] */ 
      //    __out_ecount(NumUAVs)  ID3D11UnorderedAccessView **ppUnorderedAccessViews) = 0;
      //
      //virtual void STDMETHODCALLTYPE CSGetShader( 
      //    /* [annotation] */ 
      //    __out  ID3D11ComputeShader **ppComputeShader,
      //    /* [annotation] */ 
      //    __out_ecount_opt(*pNumClassInstances)  ID3D11ClassInstance **ppClassInstances,
      //    /* [annotation] */ 
      //    __inout_opt  UINT *pNumClassInstances) = 0;
      //
      //virtual void STDMETHODCALLTYPE CSGetSamplers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_SAMPLER_SLOT_COUNT - StartSlot )  UINT NumSamplers,
      //    /* [annotation] */ 
      //    __out_ecount(NumSamplers)  ID3D11SamplerState **ppSamplers) = 0;
      //
      //virtual void STDMETHODCALLTYPE CSGetConstantBuffers( 
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - 1 )  UINT StartSlot,
      //    /* [annotation] */ 
      //    __in_range( 0, D3D11_COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT - StartSlot )  UINT NumBuffers,
      //    /* [annotation] */ 
      //    __out_ecount(NumBuffers)  ID3D11Buffer **ppConstantBuffers) = 0;
      //
      //virtual void STDMETHODCALLTYPE ClearState( void) = 0;
      //
      //virtual void STDMETHODCALLTYPE Flush( void) = 0;
      //
      //virtual D3D11_DEVICE_CONTEXT_TYPE STDMETHODCALLTYPE GetType( void) = 0;
      //
      //virtual UINT STDMETHODCALLTYPE GetContextFlags( void) = 0;
      //
      //virtual HRESULT STDMETHODCALLTYPE FinishCommandList( 
      //    BOOL RestoreDeferredContextState,
      //    /* [annotation] */ 
      //    __out_opt  ID3D11CommandList **ppCommandList) = 0;
    }

    struct BOX { public int left, top, front, right, bottom, back; }
    enum FILTER
    {
      MIN_MAG_MIP_POINT = 0,
      MIN_MAG_POINT_MIP_LINEAR = 0x1,
      MIN_POINT_MAG_LINEAR_MIP_POINT = 0x4,
      MIN_POINT_MAG_MIP_LINEAR = 0x5,
      MIN_LINEAR_MAG_MIP_POINT = 0x10,
      MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x11,
      MIN_MAG_LINEAR_MIP_POINT = 0x14,
      MIN_MAG_MIP_LINEAR = 0x15,
      ANISOTROPIC = 0x55,
      COMPARISON_MIN_MAG_MIP_POINT = 0x80,
      COMPARISON_MIN_MAG_POINT_MIP_LINEAR = 0x81,
      COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x84,
      COMPARISON_MIN_POINT_MAG_MIP_LINEAR = 0x85,
      COMPARISON_MIN_LINEAR_MAG_MIP_POINT = 0x90,
      COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x91,
      COMPARISON_MIN_MAG_LINEAR_MIP_POINT = 0x94,
      COMPARISON_MIN_MAG_MIP_LINEAR = 0x95,
      COMPARISON_ANISOTROPIC = 0xd5
    }
    enum TEXTURE_ADDRESS_MODE
    {
      WRAP = 1,
      MIRROR = 2,
      CLAMP = 3,
      BORDER = 4,
      MIRROR_ONCE = 5
    }
    struct SAMPLER_DESC
    {
      public FILTER Filter;
      public TEXTURE_ADDRESS_MODE AddressU;
      public TEXTURE_ADDRESS_MODE AddressV;
      public TEXTURE_ADDRESS_MODE AddressW;
      public float MipLODBias;
      public int MaxAnisotropy;
      public COMPARISON ComparisonFunc;
      public fixed float BorderColor[4];
      public float MinLOD;
      public float MaxLOD;
    }
    enum FILL_MODE { WIREFRAME = 2, SOLID = 3 }
    enum CULL_MODE { NONE = 1, FRONT = 2, BACK = 3 }
    struct RASTERIZER_DESC
    {
      public FILL_MODE FillMode;
      public CULL_MODE CullMode;
      public int FrontCounterClockwise;
      public int DepthBias;
      public float DepthBiasClamp;
      public float SlopeScaledDepthBias;
      public int DepthClipEnable;
      public int ScissorEnable;
      public int MultisampleEnable;
      public int AntialiasedLineEnable;
    }
    enum BLEND { ZERO = 1, ONE = 2, SRC_COLOR = 3, INV_SRC_COLOR = 4, SRC_ALPHA = 5, INV_SRC_ALPHA = 6, DEST_ALPHA = 7, INV_DEST_ALPHA = 8, DEST_COLOR = 9, INV_DEST_COLOR = 10, SRC_ALPHA_SAT = 11, BLEND_FACTOR = 14, INV_BLEND_FACTOR = 15, SRC1_COLOR = 16, INV_SRC1_COLOR = 17, SRC1_ALPHA = 18, INV_SRC1_ALPHA = 19 }
    enum BLEND_OP { ADD = 1, SUBTRACT = 2, REV_SUBTRACT = 3, MIN = 4, MAX = 5 }
    enum COLOR_WRITE_ENABLE : byte { RED = 1, GREEN = 2, BLUE = 4, ALPHA = 8, ALL = (((RED | GREEN) | BLUE) | ALPHA) }
    struct RENDER_TARGET_BLEND
    {
      public int BlendEnable;
      public BLEND SrcBlend;
      public BLEND DestBlend;
      public BLEND_OP BlendOp;
      public BLEND SrcBlendAlpha;
      public BLEND DestBlendAlpha;
      public BLEND_OP BlendOpAlpha;
      public COLOR_WRITE_ENABLE RenderTargetWriteMask;
    }
    struct BLEND_DESC
    {
      public int AlphaToCoverageEnable;
      public int IndependentBlendEnable;
      public RENDER_TARGET_BLEND RenderTarget0;
      public RENDER_TARGET_BLEND RenderTarget1;
      public RENDER_TARGET_BLEND RenderTarget2;
      public RENDER_TARGET_BLEND RenderTarget3;
      public RENDER_TARGET_BLEND RenderTarget4;
      public RENDER_TARGET_BLEND RenderTarget5;
      public RENDER_TARGET_BLEND RenderTarget6;
      public RENDER_TARGET_BLEND RenderTarget7;
    }
    enum RESOURCE_DIMENSION { UNKNOWN = 0, BUFFER = 1, TEXTURE1D = 2, TEXTURE2D = 3, TEXTURE3D = 4 }

    [ComImport, Guid("dc8e63f3-d12b-4952-b47b-5e45026a862d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IResource : IDeviceChild
    {
      void _VtblGap1_4();
      RESOURCE_DIMENSION Type { get; }
      //virtual void STDMETHODCALLTYPE SetEvictionPriority( 
      //    /* [annotation] */ 
      //    __in  UINT EvictionPriority) = 0;
      //
      //virtual UINT STDMETHODCALLTYPE GetEvictionPriority( void) = 0;
    }

    [ComImport, Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface ITexture2D : IResource
    {
      void _VtblGap1_7();
      TEXTURE2D_DESC Desc { get; }
    }

    struct RECT { public int left, top, right, bottom; }

    enum RESOURCE_MISC
    {
      GENERATE_MIPS = 0x1,
      SHARED = 0x2,
      TEXTURECUBE = 0x4,
      DRAWINDIRECT_ARGS = 0x10,
      BUFFER_ALLOW_RAW_VIEWS = 0x20,
      BUFFER_STRUCTURED = 0x40,
      RESOURCE_CLAMP = 0x80,
      SHARED_KEYEDMUTEX = 0x100,
      GDI_COMPATIBLE = 0x200
    }

    struct TEXTURE2D_DESC
    {
      public int Width;
      public int Height;
      public int MipLevels;
      public int ArraySize;
      public FORMAT Format;
      public SAMPLE_DESC SampleDesc;
      public USAGE Usage;
      public BIND BindFlags;
      public CPU_ACCESS_FLAG CPUAccessFlags;
      public RESOURCE_MISC MiscFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct BUFFER_SRV
    {
      [FieldOffset(0)]
      public int FirstElement;
      [FieldOffset(4)]
      public int ElementOffset;
      [FieldOffset(0)]
      public int NumElements;
      [FieldOffset(4)]
      public int ElementWidth;
    }
    struct BUFFEREX_SRV
    {
      public int FirstElement;
      public int NumElements;
      public int Flags; //D3D11_BUFFEREX_SRV_FLAG_RAW	= 0x1
    }
    struct TEX1D_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
    }
    struct TEX1D_ARRAY_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX2D_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
    }
    struct TEX2D_ARRAY_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX3D_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
    }
    struct TEXCUBE_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
    }
    struct TEXCUBE_ARRAY_SRV
    {
      public int MostDetailedMip;
      public int MipLevels;
      public int First2DArrayFace;
      public int NumCubes;
    }
    struct TEX2DMS_SRV
    {
      public int UnusedField_NothingToDefine;
    }
    struct TEX2DMS_ARRAY_SRV
    {
      public int FirstArraySlice;
      public int ArraySize;
    }

    enum UAV_DIMENSION { UNKNOWN = 0, BUFFER = 1, TEXTURE1D = 2, TEXTURE1DARRAY = 3, TEXTURE2D = 4, TEXTURE2DARRAY = 5, TEXTURE3D = 8 }
    enum BUFFER_UAV_FLAG { RAW = 0x1, APPEND = 0x2, COUNTER = 0x4 }
    struct BUFFER_UAV { public int FirstElement, NumElements; public BUFFER_UAV_FLAG Flags; }
    [StructLayout(LayoutKind.Explicit)]
    struct UNORDERED_ACCESS_VIEW_DESC
    {
      [FieldOffset(0)]
      public FORMAT Format;
      [FieldOffset(4)]
      public UAV_DIMENSION ViewDimension;
      [FieldOffset(8)]
      public BUFFER_UAV Buffer;
      //union 
      //  {
      //  D3D11_BUFFER_UAV Buffer;
      //  D3D11_TEX1D_UAV Texture1D;
      //  D3D11_TEX1D_ARRAY_UAV Texture1DArray;
      //  D3D11_TEX2D_UAV Texture2D;
      //  D3D11_TEX2D_ARRAY_UAV Texture2DArray;
      //  D3D11_TEX3D_UAV Texture3D;
      //  } 	;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct SHADER_RESOURCE_VIEW_DESC
    {
      [FieldOffset(0)]
      public FORMAT Format;
      [FieldOffset(4)]
      public SRV_DIMENSION ViewDimension;
      [FieldOffset(8)]
      public BUFFER_SRV Buffer;
      [FieldOffset(8)]
      public TEX1D_SRV Texture1D;
      [FieldOffset(8)]
      public TEX1D_ARRAY_SRV Texture1DArray;
      [FieldOffset(8)]
      public TEX2D_SRV Texture2D;
      [FieldOffset(8)]
      public TEX2D_ARRAY_SRV Texture2DArray;
      [FieldOffset(8)]
      public TEX2DMS_SRV Texture2DMS;
      [FieldOffset(8)]
      public TEX2DMS_ARRAY_SRV Texture2DMSArray;
      [FieldOffset(8)]
      public TEX3D_SRV Texture3D;
      [FieldOffset(8)]
      public TEXCUBE_SRV TextureCube;
      [FieldOffset(8)]
      public TEXCUBE_ARRAY_SRV TextureCubeArray;
      [FieldOffset(8)]
      public BUFFEREX_SRV BufferEx;
    }


    [StructLayout(LayoutKind.Explicit)]
    struct BUFFER_RTV
    {
      [FieldOffset(0)]
      public int FirstElement;
      [FieldOffset(4)]
      public int ElementOffset;
      [FieldOffset(0)]
      public int NumElements;
      [FieldOffset(4)]
      public int ElementWidth;
    }
    struct TEX1D_RTV
    {
      public int MipSlice;
    }
    struct TEX1D_ARRAY_RTV
    {
      public int MipSlice;
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX2D_RTV
    {
      public int MipSlice;
    }
    struct TEX2DMS_RTV
    {
      public int UnusedField_NothingToDefine;
    }
    struct TEX2D_ARRAY_RTV
    {
      public int MipSlice;
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX2DMS_ARRAY_RTV
    {
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX3D_RTV
    {
      public int MipSlice;
      public int FirstWSlice;
      public int WSize;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct RENDER_TARGET_VIEW_DESC
    {
      [FieldOffset(0)]
      public FORMAT Format;
      [FieldOffset(4)]
      public RTV_DIMENSION ViewDimension;
      [FieldOffset(8)]
      public BUFFER_RTV Buffer;
      [FieldOffset(8)]
      public TEX1D_RTV Texture1D;
      [FieldOffset(8)]
      public TEX1D_ARRAY_RTV Texture1DArray;
      [FieldOffset(8)]
      public TEX2D_RTV Texture2D;
      [FieldOffset(8)]
      public TEX2D_ARRAY_RTV Texture2DArray;
      [FieldOffset(8)]
      public TEX2DMS_RTV Texture2DMS;
      [FieldOffset(8)]
      public TEX2DMS_ARRAY_RTV Texture2DMSArray;
      [FieldOffset(8)]
      public TEX3D_RTV Texture3D;
    }

    enum DSV_DIMENSION { UNKNOWN = 0, TEXTURE1D = 1, TEXTURE1DARRAY = 2, TEXTURE2D = 3, TEXTURE2DARRAY = 4, TEXTURE2DMS = 5, TEXTURE2DMSARRAY = 6 }

    struct TEX1D_DSV
    {
      public int MipSlice;
    }
    struct TEX1D_ARRAY_DSV
    {
      public int MipSlice;
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX2D_DSV
    {
      public int MipSlice;
    }
    struct TEX2D_ARRAY_DSV
    {
      public int MipSlice;
      public int FirstArraySlice;
      public int ArraySize;
    }
    struct TEX2DMS_DSV
    {
      public int UnusedField_NothingToDefine;
    }
    struct TEX2DMS_ARRAY_DSV
    {
      public int FirstArraySlice;
      public int ArraySize;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct DEPTH_STENCIL_VIEW_DESC
    {
      [FieldOffset(0)]
      public FORMAT Format;
      [FieldOffset(4)]
      public DSV_DIMENSION ViewDimension;
      [FieldOffset(8)]
      public int Flags;
      [FieldOffset(12)]
      public TEX1D_DSV Texture1D;
      [FieldOffset(12)]
      public TEX1D_ARRAY_DSV Texture1DArray;
      [FieldOffset(12)]
      public TEX2D_DSV Texture2D;
      [FieldOffset(12)]
      public TEX2D_ARRAY_DSV Texture2DArray;
      [FieldOffset(12)]
      public TEX2DMS_DSV Texture2DMS;
      [FieldOffset(12)]
      public TEX2DMS_ARRAY_DSV Texture2DMSArray;
    }

    struct MAPPED_SUBRESOURCE
    {
      public void* pData;
      public int RowPitch;
      public int DepthPitch;
    }
    enum MAP { READ = 1, WRITE = 2, READ_WRITE = 3, WRITE_DISCARD = 4, WRITE_NO_OVERWRITE = 5 }

    [ComImport, Guid("839d1216-bb2e-412b-b7f4-a9dbebe08ed1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IView : IDeviceChild
    {
      void _VtblGap1_4();
      IResource GetResource();
    };

    enum SDK_VERSION { Current = 7 }
    enum FEATURE_LEVEL
    {
      //_9_1 = 0x9100, _9_2 = 0x9200, _9_3 = 0x9300, 
      _10_0 = 0xa000, _10_1 = 0xa100, _11_0 = 0xb000, //_11_1 = 0xb100, _12_0 = 0xc000, _12_1 = 0xc100,
    }
    enum SRV_DIMENSION { UNKNOWN = 0, BUFFER = 1, TEXTURE1D = 2, TEXTURE1DARRAY = 3, TEXTURE2D = 4, TEXTURE2DARRAY = 5, TEXTURE2DMS = 6, TEXTURE2DMSARRAY = 7, TEXTURE3D = 8, TEXTURECUBE = 9, TEXTURECUBEARRAY = 10, BUFFEREX = 11, }
    enum RTV_DIMENSION { UNKNOWN = 0, BUFFER = 1, TEXTURE1D = 2, TEXTURE1DARRAY = 3, TEXTURE2D = 4, TEXTURE2DARRAY = 5, TEXTURE2DMS = 6, TEXTURE2DMSARRAY = 7, TEXTURE3D = 8 }
    enum D3D_DRIVER_TYPE { Unknown = 0, Hardware = 1, Reference = 2, Null = 3, Software = 4, Warp = 5 }
    enum CREATE_DEVICE_FLAG { SingleThreaded = 0x1, Debug = 0x2, Switch_To_Ref = 0x4, Prevent_Internal_Threading_Optimizations = 0x8, BGRA_Support = 0x20 }
    public enum CLEAR { DEPTH = 0x1, STENCIL = 0x2 }
    enum PRIMITIVE_TOPOLOGY { UNDEFINED = 0, POINTLIST = 1, LINELIST = 2, LINESTRIP = 3, TRIANGLELIST = 4, TRIANGLESTRIP = 5, LINELIST_ADJ = 10, LINESTRIP_ADJ = 11, TRIANGLELIST_ADJ = 12, TRIANGLESTRIP_ADJ = 13, }
    [Flags]
    enum FORMAT_SUPPORT
    {
      BUFFER = 0x1,
      IA_VERTEX_BUFFER = 0x2,
      IA_INDEX_BUFFER = 0x4,
      SO_BUFFER = 0x8,
      TEXTURE1D = 0x10,
      TEXTURE2D = 0x20,
      TEXTURE3D = 0x40,
      TEXTURECUBE = 0x80,
      SHADER_LOAD = 0x100,
      SHADER_SAMPLE = 0x200,
      SHADER_SAMPLE_COMPARISON = 0x400,
      SHADER_SAMPLE_MONO_TEXT = 0x800,
      MIP = 0x1000,
      MIP_AUTOGEN = 0x2000,
      RENDER_TARGET = 0x4000,
      BLENDABLE = 0x8000,
      DEPTH_STENCIL = 0x10000,
      CPU_LOCKABLE = 0x20000,
      MULTISAMPLE_RESOLVE = 0x40000,
      DISPLAY = 0x80000,
      CAST_WITHIN_BIT_LAYOUT = 0x100000,
      MULTISAMPLE_RENDERTARGET = 0x200000,
      MULTISAMPLE_LOAD = 0x400000,
      SHADER_GATHER = 0x800000,
      BACK_BUFFER_CAST = 0x1000000,
      TYPED_UNORDERED_ACCESS_VIEW = 0x2000000,
      SHADER_GATHER_COMPARISON = 0x4000000,
      DECODER_OUTPUT = 0x8000000,
      VIDEO_PROCESSOR_OUTPUT = 0x10000000,
      VIDEO_PROCESSOR_INPUT = 0x20000000,
      VIDEO_ENCODER = 0x40000000
    }

    static SAMPLE_DESC CheckMultisample(IDevice device, FORMAT fmt, int samples)
    {
      SAMPLE_DESC desc; desc.Count = 1; desc.Quality = 0;
      for (int i = samples, q; i > 0; i--)
        if (device.CheckMultisampleQualityLevels(fmt, i, out q) == 0 && q > 0) { desc.Count = i; desc.Quality = q - 1; break; }
      return desc;
    }

    [ComImport, Guid("aec22fb8-76f3-4639-9be0-28eb43a67a2e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IObject
    {
      void dummy1();
      //virtual HRESULT STDMETHODCALLTYPE SetPrivateData( 
      //    /* [annotation][in] */ 
      //    __in  REFGUID Name,
      //    /* [in] */ UINT DataSize,
      //    /* [annotation][in] */ 
      //    __in_bcount(DataSize)  const void *pData) = 0;
      void dummy2();
      //virtual HRESULT STDMETHODCALLTYPE SetPrivateDataInterface( 
      //    /* [annotation][in] */ 
      //    __in  REFGUID Name,
      //    /* [annotation][in] */ 
      //    __in  const IUnknown *pUnknown) = 0;
      void dummy3();
      //virtual HRESULT STDMETHODCALLTYPE GetPrivateData( 
      //    /* [annotation][in] */ 
      //    __in  REFGUID Name,
      //    /* [annotation][out][in] */ 
      //    __inout  UINT *pDataSize,
      //    /* [annotation][out] */ 
      //    __out_bcount(*pDataSize)  void *pData) = 0;
      //
      [return: MarshalAs(UnmanagedType.IUnknown)]
      object GetParent([In] ref Guid riid);
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct ADAPTER_DESC
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string Description;
      public uint VendorId;
      public uint DeviceId;
      public uint SubSysId;
      public uint Revision;
      public UIntPtr DedicatedVideoMemory;
      public UIntPtr DedicatedSystemMemory;
      public UIntPtr SharedSystemMemory;
      public long AdapterLuid;
    }

    [ComImport, Guid("2411e7e1-12ac-4ccf-bd14-9798e8534dc0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IAdapter : IObject
    {
      void _VtblGap1_4();
      new void dummy1();
      //virtual HRESULT STDMETHODCALLTYPE EnumOutputs( 
      //    /* [in] */ UINT Output,
      //    /* [annotation][out][in] */ 
      //    __out  IDXGIOutput **ppOutput) = 0;
      ADAPTER_DESC Desc { get; }
      //virtual HRESULT STDMETHODCALLTYPE GetDesc( 
      //    /* [annotation][out] */ 
      //    __out  DXGI_ADAPTER_DESC *pDesc) = 0;
      new void dummy3();
      //virtual HRESULT STDMETHODCALLTYPE CheckInterfaceSupport( 
      //    /* [annotation][in] */ 
      //    __in  REFGUID InterfaceName,
      //    /* [annotation][out] */ 
      //    __out  LARGE_INTEGER *pUMDVersion) = 0;
    }

    [ComImport, Guid("3d3e0379-f9de-4d58-bb6c-18d62992f1a6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IDeviceSubObject : IObject
    {
      void _VtblGap1_4(); //IObject
                          //virtual HRESULT STDMETHODCALLTYPE GetDevice( 
                          //    /* [annotation][in] */ 
                          //    __in  REFIID riid,
                          //    /* [annotation][retval][out] */ 
                          //    __out  void **ppDevice) = 0;
    };

    [ComImport, Guid("310d36a0-d2e7-4c0a-aa04-6a9d23b8886a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface ISwapChain : IDeviceSubObject
    {
      void _VtblGap1_5();
      void Present(int SyncInterval, int Flags);
      void* GetBuffer(int Buffer, [In] in Guid riid);
      new void dummy2();
      //virtual HRESULT STDMETHODCALLTYPE SetFullscreenState( 
      //    /* [in] */ BOOL Fullscreen,
      //    /* [annotation][in] */ 
      //    __in_opt  IDXGIOutput *pTarget) = 0;
      new void dummy3();
      //virtual HRESULT STDMETHODCALLTYPE GetFullscreenState( 
      //    /* [annotation][out] */ 
      //    __out  BOOL *pFullscreen,
      //    /* [annotation][out] */ 
      //    __out  IDXGIOutput **ppTarget) = 0;
      SWAP_CHAIN_DESC Desc { get; }
      void ResizeBuffers(int BufferCount, int Width, int Height, FORMAT NewFormat, int SwapChainFlags);
      void ResizeTarget([In] MODE_DESC* Desc);
      void dummy7();
      //virtual HRESULT STDMETHODCALLTYPE GetContainingOutput( 
      //    /* [annotation][out] */ 
      //    __out  IDXGIOutput **ppOutput) = 0;
      void GetFrameStatistics(out FRAME_STATISTICS LastPresentCount); // hr not supported
      uint LastPresentCount { get; }
    }

    [ComImport, Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IDXGIDevice : IObject
    {
      void _VtblGap1_4();
      IAdapter Adapter { get; }
      new void dummy1();
      //virtual HRESULT STDMETHODCALLTYPE CreateSurface( 
      //    /* [annotation][in] */ 
      //    __in  const DXGI_SURFACE_DESC *pDesc,
      //    /* [in] */ UINT NumSurfaces,
      //    /* [in] */ DXGI_USAGE Usage,
      //    /* [annotation][in] */ 
      //    __in_opt  const DXGI_SHARED_RESOURCE *pSharedResource,
      //    /* [annotation][out] */ 
      //    __out  IDXGISurface **ppSurface) = 0;
      new void dummy2();
      //virtual HRESULT STDMETHODCALLTYPE QueryResourceResidency( 
      //    /* [annotation][size_is][in] */ 
      //    __in_ecount(NumResources)  IUnknown *const *ppResources,
      //    /* [annotation][size_is][out] */ 
      //    __out_ecount(NumResources)  DXGI_RESIDENCY *pResidencyStatus,
      //    /* [in] */ UINT NumResources) = 0;
      int GPUThreadPriority { set; get; }
    }

    [ComImport, Guid("7b7166ec-21c7-44ae-b21a-c9ae321ae369"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    interface IFactory : IObject
    {
      void _VtblGap1_4();
      [PreserveSig]
      int EnumAdapters(int i, out IAdapter p);
      void MakeWindowAssociation(IntPtr WindowHandle, MWA_NO Flags);
      new void dummy3();
      //virtual HRESULT STDMETHODCALLTYPE GetWindowAssociation( 
      //    /* [annotation][out] */ 
      //    __out  HWND *pWindowHandle) = 0;
      ISwapChain CreateSwapChain([MarshalAs(UnmanagedType.IUnknown)] object Device, SWAP_CHAIN_DESC* Desc);
      void dummy5();
      //virtual HRESULT STDMETHODCALLTYPE CreateSoftwareAdapter( 
      //    /* [in] */ HMODULE Module,
      //    /* [annotation][out] */ 
      //    __out  IDXGIAdapter **ppAdapter) = 0;
    }

    public enum FORMAT
    {
      //UNKNOWN = 0,
      //R32G32B32A32_TYPELESS = 1,
      //R32G32B32A32_FLOAT = 2,
      //R32G32B32A32_UINT = 3,
      //R32G32B32A32_SINT = 4,
      //R32G32B32_TYPELESS = 5,
      R32G32B32_FLOAT = 6,
      //R32G32B32_UINT = 7,
      //R32G32B32_SINT = 8,
      //R16G16B16A16_TYPELESS = 9,
      //R16G16B16A16_FLOAT = 10,
      //R16G16B16A16_UNORM = 11,
      //R16G16B16A16_UINT = 12,
      //R16G16B16A16_SNORM = 13,
      //R16G16B16A16_SINT = 14,
      //R32G32_TYPELESS = 15,
      R32G32_FLOAT = 16,
      //R32G32_UINT = 17,
      //R32G32_SINT = 18,
      //R32G8X24_TYPELESS = 19,
      //D32_FLOAT_S8X24_UINT = 20,
      //R32_FLOAT_X8X24_TYPELESS = 21,
      //X32_TYPELESS_G8X24_UINT = 22,
      //R10G10B10A2_TYPELESS = 23,
      //R10G10B10A2_UNORM = 24,
      //R10G10B10A2_UINT = 25,
      //R11G11B10_FLOAT = 26,
      //R8G8B8A8_TYPELESS = 27,
      //R8G8B8A8_UNORM = 28,
      //R8G8B8A8_UNORM_SRGB = 29,
      //R8G8B8A8_UINT = 30,
      //R8G8B8A8_SNORM = 31,
      //R8G8B8A8_SINT = 32,
      //R16G16_TYPELESS = 33,
      //R16G16_FLOAT = 34,
      //R16G16_UNORM = 35,
      //R16G16_UINT = 36,
      //R16G16_SNORM = 37,
      //R16G16_SINT = 38,
      //R32_TYPELESS = 39,
      //D32_FLOAT = 40,
      //R32_FLOAT = 41,
      //R32_UINT = 42,
      //R32_SINT = 43,
      //R24G8_TYPELESS = 44,
      D24_UNORM_S8_UINT = 45,
      //R24_UNORM_X8_TYPELESS = 46,
      //X24_TYPELESS_G8_UINT = 47,
      //R8G8_TYPELESS = 48,
      //R8G8_UNORM = 49,
      //R8G8_UINT = 50,
      //R8G8_SNORM = 51,
      //R8G8_SINT = 52,
      //R16_TYPELESS = 53,
      //R16_FLOAT = 54,
      //D16_UNORM = 55,
      //R16_UNORM = 56,
      R16_UINT = 57,
      //R16_SNORM = 58,
      //R16_SINT = 59,
      //R8_TYPELESS = 60,
      //R8_UNORM = 61,
      //R8_UINT = 62,
      //R8_SNORM = 63,
      //R8_SINT = 64,
      A8_UNORM = 65,
      //R1_UNORM = 66,
      //R9G9B9E5_SHAREDEXP = 67,
      //R8G8_B8G8_UNORM = 68,
      //G8R8_G8B8_UNORM = 69,
      //BC1_TYPELESS = 70,
      //BC1_UNORM = 71,
      //BC1_UNORM_SRGB = 72,
      //BC2_TYPELESS = 73,
      //BC2_UNORM = 74,
      //BC2_UNORM_SRGB = 75,
      //BC3_TYPELESS = 76,
      //BC3_UNORM = 77,
      //BC3_UNORM_SRGB = 78,
      //BC4_TYPELESS = 79,
      //BC4_UNORM = 80,
      //BC4_SNORM = 81,
      //BC5_TYPELESS = 82,
      //BC5_UNORM = 83,
      //BC5_SNORM = 84,
      B5G6R5_UNORM = 85,
      //B5G5R5A1_UNORM = 86,
      B8G8R8A8_UNORM = 87,
      B8G8R8X8_UNORM = 88,
      //R10G10B10_XR_BIAS_A2_UNORM = 89,
      //B8G8R8A8_TYPELESS = 90,
      //B8G8R8A8_UNORM_SRGB = 91,
      //B8G8R8X8_TYPELESS = 92,
      //B8G8R8X8_UNORM_SRGB = 93,
      //BC6H_TYPELESS = 94,
      //BC6H_UF16 = 95,
      //BC6H_SF16 = 96,
      //BC7_TYPELESS = 97,
      //BC7_UNORM = 98,
      //BC7_UNORM_SRGB = 99,
    }

    enum MODE_SCANLINE_ORDER { UNSPECIFIED = 0, PROGRESSIVE = 1, UPPER_FIELD_FIRST = 2, LOWER_FIELD_FIRST = 3 }
    enum MODE_SCALING { UNSPECIFIED = 0, CENTERED = 1, STRETCHED = 2 }
    enum BUFFERUSAGE { SHADER_INPUT = (1 << (0 + 4)), RENDER_TARGET_OUTPUT = (1 << (1 + 4)), BACK_BUFFER = (1 << (2 + 4)), SHARED = (1 << (3 + 4)), READ_ONLY = (1 << (4 + 4)), DISCARD_ON_PRESENT = (1 << (5 + 4)), UNORDERED_ACCESS = (1 << (6 + 4)), }
    enum SWAP_EFFECT { DISCARD = 0, SEQUENTIAL = 1 }
    enum MWA_NO { WINDOW_CHANGES = (1 << 0), ALT_ENTER = (1 << 1), PRINT_SCREEN = (1 << 2) }

    struct RATIONAL
    {
      public int Numerator;
      public int Denominator;
    }
    struct MODE_DESC
    {
      public int Width;
      public int Height;
      public RATIONAL RefreshRate;
      public FORMAT Format;
      public MODE_SCANLINE_ORDER ScanlineOrdering;
      public MODE_SCALING Scaling;
    }
    struct SAMPLE_DESC
    {
      public int Count;
      public int Quality;
    }
    struct SWAP_CHAIN_DESC
    {
      public MODE_DESC BufferDesc;
      public SAMPLE_DESC SampleDesc;
      public BUFFERUSAGE BufferUsage;
      public int BufferCount;
      public IntPtr OutputWindow;
      public int Windowed;
      public SWAP_EFFECT SwapEffect;
      public int Flags;
    }
    struct FRAME_STATISTICS
    {
      public uint PresentCount;
      public uint PresentRefreshCount;
      public uint SyncRefreshCount;
      public long SyncQPCTime;
      public long SyncGPUTime;
    }
  }
}