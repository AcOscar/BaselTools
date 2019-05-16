''' <summary>
''' Elliptical Mercator projection
''' </summary>
''' <remarks>source: http://wiki.openstreetmap.org/wiki/Mercator#C.23 translated via http://www.carlosag.net/Tools/CodeTranslator/ 
''' and some manual optimizations by Holger Rasch</remarks>
Public Class OSM_MercatorProjection

    'Private Shared R_MAJOR As Double = 6378206.4 'Clarke 1866
    Const R_MAJOR As Double = 6378137

    'Private Shared R_MINOR As Double = 6356752.3142 'orginal in the source code
    Const R_MINOR As Double = 6356752.31414 'GRS80
    'Private Shared R_MINOR As Double = 6356752.31424518 'GRS84
    'Private Shared R_MINOR As Double = 6356583.8 'Clarke 1866

    Const RATIO As Double = R_MINOR / R_MAJOR

    Const DEG2RAD As Double = Math.PI / 180

    Const RAD2Deg As Double = 180 / Math.PI

    Const PI_2 As Double = Math.PI / 2

    Private Shared ECCENT As Double = Math.Sqrt(1 - RATIO ^ 2)

    Private Shared COM As Double = 0.5 * ECCENT


    Public Shared Function toPixel(ByVal lon As Double, ByVal lat As Double) As Double()

        Return New Double() {lonToX(lon), latToY(lat)}

    End Function

    Public Shared Function toGeoCoord(ByVal x As Double, ByVal y As Double) As Double()

        Return New Double() {xToLon(x), yToLat(y)}

    End Function

    Public Shared Function lonToX(ByVal lon As Double) As Double

        Return R_MAJOR * DegToRad(lon)

    End Function

    Public Shared Function latToY(ByVal lat As Double) As Double

        lat = Math.Min(89.5, Math.Max(lat, -89.5))

        Dim phi As Double = DegToRad(lat)

        Dim sinphi As Double = Math.Sin(phi)

        Dim con As Double = (ECCENT * sinphi)

        con = Math.Pow(((1 - con) / (1 + con)), COM)

        Dim ts As Double = (Math.Tan((0.5 * ((Math.PI * 0.5) - phi))) / con)

        Return (0 - (R_MAJOR * Math.Log(ts)))

    End Function

    Public Shared Function xToLon(ByVal x As Double) As Double

        Return RadToDeg(x) / R_MAJOR

    End Function

    Public Shared Function yToLat(ByVal y As Double) As Double

        Dim ts As Double = Math.Exp(((y / R_MAJOR) * -1))

        Dim phi As Double = (PI_2 - (2 * Math.Atan(ts)))

        Dim dphi As Double = 1

        Dim i As Integer = 0

        While (Math.Abs(dphi) > 0.000000001) AndAlso (i < 15)

            Dim con As Double = ECCENT * Math.Sin(phi)

            dphi = PI_2 - ((2 * Math.Atan((ts * Math.Pow(((1 - con) / (1 + con)), COM)))) - phi)

            phi = phi + dphi

            i += 1

        End While

        Return RadToDeg(phi)

    End Function

    Private Shared Function RadToDeg(ByVal rad As Double) As Double

        Return rad * RAD2Deg

    End Function

    Private Shared Function DegToRad(ByVal deg As Double) As Double

        Return deg * DEG2RAD

    End Function

End Class