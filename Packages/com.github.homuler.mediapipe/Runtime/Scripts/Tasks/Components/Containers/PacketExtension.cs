// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;

namespace Mediapipe.Tasks.Components.Containers
{
  public static class PacketExtension
  {
    public static void GetNormalizedLandmarksList(this Packet packet, List<NormalizedLandmarks> outs)
    {
      UnsafeNativeMethods.mp_Packet__GetNormalizedLandmarksVector(packet.mpPtr, out var landmarksArray).Assert();
      outs.FillWith(landmarksArray);
      landmarksArray.Dispose();
    }
  }
}