Imports System.Text.RegularExpressions
Imports System.String
Imports System.Text
Imports System.Web
Imports System.Net
Public Class Form1
    Dim Profilepicture, imagepost As Byte()
    Dim profilepic_mime_type, imagepost_mime_type As String
    Dim cookielist As New List(Of String)
    Dim doneprofile, doneimage, errorprofile, errorimage As Integer
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        BackgroundWorker2.RunWorkerAsync()
    End Sub

#Region "Change Profile pictrue"
    Function change_profile_pic(pic As Byte(), cookies_ As String, mime_type As String)
        Dim tok As String = RandomString(16)
        Using w As New Net.WebClient
            w.Headers.Add("Accept: */*")
            w.Headers.Add("Content-Type: multipart/form-data; boundary=----WebKitFormBoundary" + tok)
            w.Headers.Add("Cookie", cookies_)
            w.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36")
            w.Headers.Add("X-CSRFToken", Regex.Match(cookies_, "csrftoken=(.*?);").Groups(1).Value)
            w.Headers.Add("X-IG-App-ID: 936619743392459")
            w.Headers.Add("X-Instagram-AJAX: 1")
            w.Headers.Add("X-Requested-With: XMLHttpRequest")
            Dim data_post As New StringBuilder
            data_post.AppendLine("------WebKitFormBoundary" + tok)
            data_post.AppendLine("Content-Disposition: form-data; name=""profile_pic""; filename=""profilepic.jpg""")
            data_post.AppendLine("Content-Type: " + mime_type)
            data_post.AppendLine(String.Empty)
            data_post.AppendLine(System.Text.Encoding.Default.GetString(pic).ToString)
            data_post.AppendLine(String.Format("------WebKitFormBoundary{0}--", tok))
            Try
                Dim rep As String = w.UploadString("https://i.instagram.com/accounts/web_change_profile_picture/", "POST", data_post.ToString)
                Dim Stringrsp As String = rep
                If Stringrsp.Contains("""changed_profile"": true") Then
                    Return True
                End If
            Catch ex As Net.WebException
                MsgBox(ex.Message)
                MsgBox(New IO.StreamReader(ex.Response.GetResponseStream).ReadToEnd)
            End Try

        End Using
        Return False
    End Function
#End Region
    Function RandomString(ByRef Length As String) As String
        Dim str As String = Nothing
        Dim rnd As New Random
        For i As Integer = 0 To Length
            Dim chrInt As Integer = 0
            Do
                chrInt = rnd.Next(30, 122)
                If (chrInt >= 48 And chrInt <= 57) Or (chrInt >= 65 And chrInt <= 90) Or (chrInt >= 97 And chrInt <= 122) Then
                    Exit Do
                End If
            Loop
            str &= Chr(chrInt)
        Next
        Return str
    End Function
    Dim listacc() As String
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim op As New OpenFileDialog() With {.Filter = "Text File|*.txt", .Multiselect = False, .Title = "List User:Pass", .InitialDirectory = Application.StartupPath}
        If op.ShowDialog = DialogResult.OK Then
            listacc = IO.File.ReadAllLines(op.FileName)
            DirectCast(sender, Button).ForeColor = Color.Green
            BackgroundWorker1.RunWorkerAsync()
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim op As New OpenFileDialog() With {.Filter = "Image|*.jpg; *.png; *.jfif", .Multiselect = False, .Title = "ProfilePicture Image"}
        If op.ShowDialog = DialogResult.OK Then
            If MimeMapping.GetMimeMapping(op.FileName).Contains("image") Then
                Profilepicture = IO.File.ReadAllBytes(op.FileName)
                profilepic_mime_type = MimeMapping.GetMimeMapping(op.FileName)
                DirectCast(sender, Button).ForeColor = Color.Green
            End If

        End If
    End Sub
    Dim Widthimg, Heightimg As Integer

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        For Each acc As String In listacc
            Dim combo_() As String = acc.Split(":")
            If login(combo_(0), combo_(1)) Then
                ListBox1.Invoke(Sub() ListBox1.Items.Add(combo_(0)))
            End If

        Next
        MsgBox("Done Add Accounts")
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        MsgBox(DirectCast(sender, Button).Text)
    End Sub


    Private Sub BackgroundWorker2_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        If cookielist.Count = Nothing Then
            MsgBox("Add Account")
            Exit Sub
        End If
        Button5.Invoke(Sub() Button5.Enabled = False)
        For Each cookie As String In cookielist
            If CheckBox1.Checked Then
                If change_profile_pic(Profilepicture, cookie, profilepic_mime_type) Then
                    doneprofile += 1
                Else
                    errorprofile += 1
                End If
            End If
            If CheckBox2.Checked Then
                Try
                    Dim micro = (DateTime.Now - New DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds.ToString.Split(".")
                    '  MsgBox(micro(0))
                    If uploud_img(cookie, micro(0), imagepost, Heightimg, Widthimg) Then
                        doneimage += 1
                    Else
                        errorimage += 1
                    End If
                Catch ex As Exception
                    MsgBox("Error", MsgBoxStyle.Critical, "")
                End Try
            End If
            Label4.Invoke(Sub() Label4.Text = String.Format("Done Upload picture: {0}", doneimage))
            Label5.Invoke(Sub() Label5.Text = String.Format("Error Upload picture: {0}", errorimage))
            Label6.Invoke(Sub() Label6.Text = String.Format("Done Change Profile_pic: {0}", doneprofile))
            Label7.Invoke(Sub() Label7.Text = String.Format("Error Change Profile_pic: {0}", errorprofile))
        Next
        Button5.Invoke(Sub() Button5.Enabled = True)
        MsgBox("Done!")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim op As New OpenFileDialog() With {.Filter = "Image|*.jpg; *.png; *.jfif", .Multiselect = False, .Title = "Post Image"}
        If op.ShowDialog = DialogResult.OK Then
            Dim bmp As New Bitmap(op.FileName)
            If Not MimeMapping.GetMimeMapping(op.FileName).Contains("jpg") Then
                Dim ms = New IO.MemoryStream()
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg)
                imagepost = ms.ToArray()
                Widthimg = bmp.Width
                Heightimg = bmp.Height
            Else
                Widthimg = bmp.Width
                Heightimg = bmp.Height
                imagepost = IO.File.ReadAllBytes(op.FileName)
                imagepost_mime_type = MimeMapping.GetMimeMapping(op.FileName)
                DirectCast(sender, Button).ForeColor = Color.Green
            End If
            DirectCast(sender, Button).ForeColor = Color.Green
        End If

    End Sub
#Region "Login"
    Function login(Username, Password) As Boolean
        ' By ...RXLib.dll
        Try
            Dim csrftoken As String = get_token()
            Dim postData As String = "username=" & Username & "&enc_password=" & "#PWD_INSTAGRAM_BROWSER:0:" & (DateTime.Now - New DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds.ToString.Split()(0) & ":" & Password
            Dim tempcook As New CookieContainer
            Dim en As New UTF8Encoding
            Dim byteData As Byte() = en.GetBytes(postData)

            Dim httpPost = DirectCast(WebRequest.Create("https://www.instagram.com/accounts/login/ajax/"), HttpWebRequest)

            httpPost.Method = "POST"
            httpPost.KeepAlive = True
            httpPost.ContentType = "application/x-www-form-urlencoded"
            httpPost.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36"
            httpPost.ContentLength = byteData.Length
            httpPost.Headers.Add("x-csrftoken", csrftoken)
            httpPost.Headers.Add("X-Instagram-AJAX", "1")
            httpPost.Headers.Add("x-requested-with", "XMLHttpRequest")

            'Send Data
            Dim poststr As IO.Stream = httpPost.GetRequestStream()
            poststr.Write(byteData, 0, byteData.Length)
            poststr.Close()

            'Get Response
            Dim POST_Response As HttpWebResponse
            POST_Response = DirectCast(httpPost.GetResponse(), HttpWebResponse)

            Dim tt As String
            tt = POST_Response.Headers("Set-Cookie")

            Dim cookie = Regex.Match(tt, "ig_did=(.*?;|$)").Value & " " & Regex.Match(tt, "csrftoken=.*?[;|$]").Value & " " & Regex.Match(tt, "mid=.*?[;|$]").Value & " " & Regex.Match(tt, "ds_user_id=.*?[;|$]").Value & " " & Regex.Match(tt, "sessionid=.*?[;|$]").Value
            Dim Post_Reader As New IO.StreamReader(POST_Response.GetResponseStream())
            Dim Response As String = Post_Reader.ReadToEnd
            If Response.Contains("authenticated"": true") Then
                cookielist.Add(cookie)
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function
    Function get_token() As String

        Try
            Dim httpGet = DirectCast(WebRequest.Create("https://www.instagram.com/"), HttpWebRequest)
            httpGet.Method = "GET"
            'httpGet.ContentType = ""
            httpGet.Headers.Add("X-Instagram-AJAX", "1")
            httpGet.Headers.Add("x-requested-with", "XMLHttpRequest")

            Dim Get_Response As HttpWebResponse
            Get_Response = DirectCast(httpGet.GetResponse(), HttpWebResponse)

            Dim Get_Reader As New IO.StreamReader(Get_Response.GetResponseStream())

            Return Regex.Match(Get_Reader.ReadToEnd, """csrf_token"":""(\w+)""").Groups(1).Value

        Catch ex As Exception
            Return "missing"
        End Try
    End Function
#End Region
#Region "Upload Image"
    Function uploud_img(acc_cookies As String, time_nowis As String, imgbyts As Byte(), imgHG As String, imgwd As String) As Boolean
        Dim Rcs As New Regex("csrftoken=(\w+)(;|$)")
        Dim csrftoken = Rcs.Match(acc_cookies).Groups(1).Value
        Try
            Dim en As New UTF8Encoding
            ' Dim postData As String = Encoding.Default.GetString(imgby)
            Dim tempcook As New CookieContainer

            Dim byteData As Byte() = imgbyts

            Dim httpPost = DirectCast(WebRequest.Create($"https://www.instagram.com/rupload_igphoto/fb_uploader_{time_nowis}"), HttpWebRequest)

            httpPost.Host = "i.instagram.com"
            httpPost.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36"
            httpPost.Accept = "*/*"
            httpPost.Method = "POST"
            httpPost.KeepAlive = True
            httpPost.ContentType = "image/jpeg"
            httpPost.ContentLength = byteData.Length
            httpPost.Headers.Add("Cookie", acc_cookies)
            httpPost.Headers.Add("X-Instagram-Rupload-Params: {""media_type"":1,""upload_id"":""" & time_nowis & """,""upload_media_height"":" & imgHG & ",""upload_media_width"":" & imgwd & "}")
            httpPost.Headers.Add("X-Entity-Name: fb_uploader_" & time_nowis)
            httpPost.Headers.Add("X-Entity-Length", byteData.Length.ToString)
            httpPost.Headers.Add("x-csrftoken", csrftoken)
            httpPost.Headers.Add("X-Instagram-AJAX", "1")
            httpPost.Headers.Add("x-requested-with", "XMLHttpRequest")
            httpPost.Headers.Add("Offset: 0")
            'Send Data
            Dim poststr As IO.Stream = httpPost.GetRequestStream()
            poststr.Write(byteData, 0, byteData.Length)
            poststr.Close()

            'Get Response
            Dim POST_Response As HttpWebResponse
            POST_Response = DirectCast(httpPost.GetResponse(), HttpWebResponse)

            Dim Post_Reader As New IO.StreamReader(POST_Response.GetResponseStream())
            Dim Response As String = Post_Reader.ReadToEnd
            Debug.WriteLine(Response)
            If share_it(acc_cookies, time_nowis) Then
                Return True
            End If
        Catch ex As WebException : End Try
        Return False
    End Function
    Function share_it(acc_cookies, id_time) As Boolean
        Dim Rcs As New Regex("csrftoken=(\w+)(;|$)")
        Dim csrftoken = Rcs.Match(acc_cookies).Groups(1).Value
        Try
            Dim en As New UTF8Encoding
            Dim postData As String = "upload_id=" & id_time & "&caption=" & TextBox1.Text.Replace(vbCrLf, "%0D%0A").Replace(" ", "+") & "&usertags=&custom_accessibility_caption=&retry_timeout="
            Dim tempcook As New CookieContainer

            Dim byteData As Byte() = en.GetBytes(postData)


            Dim httpPost = DirectCast(WebRequest.Create($"https://www.instagram.com/create/configure/"), HttpWebRequest)

            httpPost.Host = "i.instagram.com"
            httpPost.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36"
            httpPost.Accept = "*/*"
            httpPost.Method = "POST"
            httpPost.KeepAlive = True
            httpPost.ContentType = "application/x-www-form-urlencoded"
            httpPost.ContentLength = byteData.Length
            httpPost.Headers.Add("Cookie", acc_cookies)
            httpPost.Headers.Add("x-csrftoken", csrftoken)
            httpPost.Headers.Add("X-Instagram-AJAX", "1")
            httpPost.Headers.Add("x-requested-with", "XMLHttpRequest")
            'Send Data
            Dim poststr As IO.Stream = httpPost.GetRequestStream()
            poststr.Write(byteData, 0, byteData.Length)
            poststr.Close()

            'Get Response
            Dim POST_Response As HttpWebResponse
            POST_Response = DirectCast(httpPost.GetResponse(), HttpWebResponse)

            Dim Post_Reader As New IO.StreamReader(POST_Response.GetResponseStream())
            Dim Response As String = Post_Reader.ReadToEnd
            If Response.Contains("""status"": ""ok") Then
                Return True
            Else
                Return False
            End If
        Catch ex As WebException
            Dim rsponString As String = New IO.StreamReader(ex.Response.GetResponseStream).ReadToEnd
            If rsponString IsNot Nothing Then
                MsgBox(rsponString, MsgBoxStyle.Critical)
            End If
            Return False
        End Try
    End Function
#End Region
End Class
