; Inno Setup Script for LessonScheduler

#define MyAppName "Lesson Scheduler"
#define MyAppVersion "4.0"
#define MyAppPublisher "Lesson Scheduler Team"
#define MyAppURL "https://github.com/yourusername/lesson-scheduler"
#define MyAppExeName "LessonScheduler.exe"
#define VCRedistUrl "https://aka.ms/vs/17/release/vc_redist.x64.exe"

[Setup]
; Buraya kendi bilgilerinizi girin
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; LicenseFile=LICENSE.txt
; InfoBeforeFile=README.txt
OutputBaseFilename=LessonScheduler_Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[CustomMessages]
turkish.VCRedistRequired=Bu program çalışmak için Visual C++ Redistributable 2022 gerektirir.
turkish.VCRedistInstallPrompt=Şimdi yüklemek ister misiniz?
turkish.VCRedistInstallNote=Not: Kurulum için yönetici izni gerekebilir.
turkish.VCRedistInstallFailed=VC++ Redistributable kurulum dosyası çalıştırılamadı!
turkish.VCRedistInstallError=VC++ Redistributable kurulumu tamamlanamadı
turkish.VCRedistInstallSuccess=VC++ Redistributable başarıyla yüklendi!
turkish.VCRedistSkipped=VC++ Redistributable kurulumu atlandı.
turkish.VCRedistWarning=Program düzgün çalışmayabilir!
turkish.ContinueAnyway=Yine de kuruluma devam etmek istiyor musunuz?
turkish.ManualInstall=Manuel kurulum için:

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Files]
; VC++ Redistributable dosyasını dahil et
Source: "VC_redist.x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

; Ana program exe dosyası (tek dosya, sıkıştırılmış - 80MB)
Source: "LessonScheduler.exe"; DestDir: "{app}"; Flags: ignoreversion

; test5.json dosyası
Source: "test5.json"; DestDir: "{app}"; Flags: ignoreversion

; Tek dosya exe olduğu için DLL'ler gerekli değil
; Source: "bin\Release\net8.0-windows\win-x64\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion

; İkon dosyası
Source: "school.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
; VC++ Redistributable kontrolü ve kurulumu
Filename: "{tmp}\VC_redist.x64.exe"; Parameters: "/quiet /norestart"; StatusMsg: "VC++ Redistributable yükleniyor..."; Flags: waituntilterminated; Check: NeedsVCRedistributable

; Programı çalıştır
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Kaldırma işleminden sonra temp dosyalarını temizle
Type: filesandordirs; Name: "{app}\*.json"

[Code]
// VC++ Redistributable 2022 kontrolü fonksiyonu
function IsVCRedist2022Installed(): Boolean;
var
  Version: String;
  Major, Minor, Build: Integer;
begin
  Result := False;
  
  // Visual Studio 2022 VC++ Redistributable x64 kontrolü
  if RegQueryStringValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Version', Version) then
  begin
    // Version format: v14.xx.xxxxx (örn: v14.32.31332)
    // Visual Studio 2022 için minimum v14.30 gerekli
    if Length(Version) > 3 then
    begin
      try
        Major := StrToInt(Copy(Version, 2, 2)); // "14" kısmı
        if Major >= 14 then
        begin
          Minor := StrToInt(Copy(Version, 5, 2)); // "xx" kısmı  
          if Minor >= 30 then // VS 2022 minimum
            Result := True;
        end;
      except
        Result := False;
      end;
    end;
  end;
end;

// VC++ Redistributable gerekli mi kontrolü
function NeedsVCRedistributable(): Boolean;
begin
  Result := not IsVCRedist2022Installed();
end;

// Kurulum öncesi VC++ Redistributable kontrolü
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  
  // VC++ Redistributable kontrolü
  if NeedsVCRedistributable() then
  begin
    if MsgBox(ExpandConstant('{cm:VCRedistRequired}') + #13#10 + 
              ExpandConstant('{cm:VCRedistInstallPrompt}') + #13#10#13#10 + 
              ExpandConstant('{cm:VCRedistInstallNote}'), 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      // VC++ Redistributable kurulumu
      if not Exec(ExpandConstant('{tmp}\VC_redist.x64.exe'), '/quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      begin
        MsgBox(ExpandConstant('{cm:VCRedistInstallFailed}') + #13#10 + 
               ExpandConstant('{cm:ManualInstall}') + ' https://aka.ms/vs/17/release/vc_redist.x64.exe', 
               mbCriticalError, MB_OK);
        Result := False;
      end
      else if ResultCode <> 0 then
      begin
        MsgBox(ExpandConstant('{cm:VCRedistInstallError}') + ' (Hata kodu: ' + IntToStr(ResultCode) + ')' + #13#10 + 
               ExpandConstant('{cm:VCRedistWarning}') + ' ' + ExpandConstant('{cm:ManualInstall}') + #13#10 + 
               'https://aka.ms/vs/17/release/vc_redist.x64.exe', 
               mbError, MB_OK);
        // Kuruluma devam et ama uyar
      end
      else
      begin
        MsgBox(ExpandConstant('{cm:VCRedistInstallSuccess}'), mbInformation, MB_OK);
      end;
    end
    else
    begin
      if MsgBox(ExpandConstant('{cm:VCRedistSkipped}') + #13#10 + 
                ExpandConstant('{cm:VCRedistWarning}') + #13#10#13#10 + 
                ExpandConstant('{cm:ContinueAnyway}'), 
                mbConfirmation, MB_YESNO) = IDNO then
      begin
        Result := False;
      end;
    end;
  end;
end;