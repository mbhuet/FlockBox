## 1.2.3 (2020-07-26)
    - Fixed solid drawing not working when using VR rendering.
    - Fixed nothing was visible when using the Universal Render Pipeline and post processing was enabled.
        Note that ALINE will render before post processing effects when using the URP.
        This is because as far as I can tell the Universal Render Pipeline does not expose any way to render objects
        after post processing effects because it renders to hidden textures that custom passes cannot access.
    - Fixed drawing sometimes not working when using the High Definition Render Pipeline.
        In contrast to the URP, ALINE can actually render after post processing effects with the HDRP since it has a nicer API. So it does that.
    - Known bugs
        - \reflink{Draw.SolidMesh(Mesh)} does not respect matrices and will always be drawn with the pivot at the world origin.

## 1.2.2 (2020-07-11)
    - Added \reflink{Draw.Arc(float3,float3,float3)}.
        \shadowimage{rendered/arc.png}
    - Fixed drawing sometimes not working when using the Universal Render Pipeline, in particular when either HDR or anti-aliasing was enabled.
    - Fixed drawing not working when using VR rendering.
    - Hopefully fixed the issue that could sometimes cause "The ALINE package installation seems to be corrupt. Try reinstalling the package." to be logged when first installing
        the package (even though the package wasn't corrupt at all).
    - Incremented required burst package version from 1.3.0-preview.7 to 1.3.0.
    - Fixed the offline documentation showing the wrong page instead of the get started guide.

## 1.2.1 (2020-06-21)
    - Breaking changes
        - Changed the size parameter of Draw.WireRect to be a float2 instead of a float3.
            It made no sense for it to be a float3 since a rectangle is two-dimensional. The y coordinate of the parameter was never used.
    - Added <a href="ref:Draw.WirePlane(float3,float3,float2)">Draw.WirePlane</a>.
        \shadowimage{rendered/wireplane.png}
    - Added <a href="ref:Draw.SolidPlane(float3,float3,float2)">Draw.SolidPlane</a>.
        \shadowimage{rendered/solidplane.png}
    - Added <a href="ref:Draw.PlaneWithNormal(float3,float3,float2)">Draw.PlaneWithNormal</a>.
        \shadowimage{rendered/planewithnormal.png}
    - Fixed Drawing.DrawingUtilities class missed an access modifier. Now all methods are properly public and can be accessed without any issues.
    - Fixed an error could be logged after using the WireMesh method and then exiting/entering play mode.
    - Fixed Draw.Arrow not drawing the arrowhead properly when the arrow's direction was a multiple of (0,1,0).

## 1.2 (2020-05-22)
    - Added page showing some advanced usages: \ref advanced.
    - Added \link Drawing.Draw.WireMesh Draw.WireMesh\endlink.
        \shadowimage{rendered/wiremesh.png}
    - Added \link Drawing.CommandBuilder.cameraTargets CommandBuilder.cameraTargets\endlink.
    - The WithDuration scope can now be used even outside of play mode. Outside of play mode it will use Time.realtimeSinceStartup to measure the duration.
    - The WithDuration scope can now be used inside burst jobs and on different threads.
    - Fixed WireCylinder and WireCapsule logging a warning if the normalized direction from the start to the end was exactly (1,1,1).normalized. Thanks Billy Attaway for reporting this.
    - Fixed the documentation showing the wrong namespace for classes. It listed \a Pathfinding.Drawing but the correct namespace is just \a %Drawing.

## 1.1.1 (2020-05-04)
    - Breaking changes
        - The vertical alignment of Label2D has changed slightly. Previously the Top and Center alignments were a bit off from the actual top/center.
    - Fixed conflicting assembly names when used in a project that also has the A* Pathfinding Project package installed.
    - Fixed a crash when running on iOS.
    - Improved alignment of \link Drawing.Draw.Label2D Draw.Label2D\endlink when using the Top or Center alignment.

## 1.1 (2020-04-20)
    - Added \link Drawing.Draw.Label2D Draw.Label2D\endlink which allows you to easily render text from your code.
        It uses a signed distance field font renderer which allows you to render crisp text even at high resolution.
        At very small font sizes it falls back to a regular font texture.
        \shadowimage{rendered/label2d.png}
    - Improved performance of drawing lines by about 5%.
    - Fixed a potential crash after calling the Draw.Line(Vector3,Vector3,Color) method.

## 1.0.2 (2020-04-09)
    - Breaking changes
        - A few breaking changes may be done as the package matures. I strive to keep these to as few as possible, while still not sacrificing good API design.
        - Changed the behaviour of \link Drawing.Draw.Arrow(float3,float3,float3,float) Draw.Arrow\endlink to use an absolute size head.
            This behaviour is probably the desired one more often when one wants to explicitly set the size.
            The default Draw.Arrow(float3,float3) function which does not take a size parameter continues to use a relative head size of 20% of the length of the arrow.
            \shadowimage{rendered/arrow_multiple.png}
    - Added \link Drawing.Draw.ArrowRelativeSizeHead Draw.ArrowRelativeSizeHead\endlink which uses a relative size head.
        \shadowimage{rendered/arrowrelativesizehead.png}
    - Added \link Drawing.DrawingManager.GetBuilder DrawingManager.GetBuilder\endlink instead of the unnecessarily convoluted DrawingManager.instance.gizmos.GetBuilder.
    - Added \link Drawing.Draw.CatmullRom(List<Vector3>) Draw.CatmullRom\endlink for drawing a smooth curve through a list of points.
        \shadowimage{rendered/catmullrom.png}
    - Made it easier to draw things that are visible in standalone games. You can now use for example Draw.ingame.WireBox(Vector3.zero, Vector3.one) instead of having to create a custom command builder.
        See \ref ingame for more details.

## 1.0.1 (2020-04-06)
    - Fix burst example scene not having using burst enabled (so it was much slower than it should have been).
    - Fix text color in the SceneEditor example scene was so dark it was hard to read.
    - Various minor documentation fixes.

## 1.0 (2020-04-05)
    - Initial release
