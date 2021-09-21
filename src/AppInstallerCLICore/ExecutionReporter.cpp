// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#include "pch.h"
#include "ExecutionReporter.h"


namespace AppInstaller::CLI::Execution
{
    using namespace Settings;
    using namespace VirtualTerminal;

    const Sequence& HelpCommandEmphasis = TextFormat::Foreground::Bright;
    const Sequence& HelpArgumentEmphasis = TextFormat::Foreground::Bright;
    const Sequence& ManifestInfoEmphasis = TextFormat::Foreground::Bright;
    const Sequence& SourceInfoEmphasis = TextFormat::Foreground::Bright;
    const Sequence& NameEmphasis = TextFormat::Foreground::BrightCyan;
    const Sequence& IdEmphasis = TextFormat::Foreground::BrightCyan;
    const Sequence& UrlEmphasis = TextFormat::Foreground::BrightBlue;
    const Sequence& PromptEmphasis = TextFormat::Foreground::Bright;

    Reporter::Reporter(std::ostream& outStream, std::istream& inStream) :
        m_out(outStream),
        m_in(inStream),
        m_vtOutputStream(m_out),
        m_nonVTOutputStream(m_out),
        m_progressBar(std::in_place, m_vtOutputStream, IsVTEnabled()),
        m_spinner(std::in_place, m_vtOutputStream, IsVTEnabled())
    {
        SetProgressSink(this);
        SetChannel(Channel::Output);
    }

    Reporter::~Reporter()
    {
        CloseOutputStream();
    }

    Reporter::Reporter(const Reporter& other, clone_t) :
        Reporter(other.m_out, other.m_in)
    {
        if (other.m_style.has_value())
        {
            SetStyle(*other.m_style);
        }
    }

    void Reporter::SetChannel(Channel channel)
    {
        m_channel = channel;

        if (m_channel == Channel::Output)
        {
            m_vtOutputStream.Enable();
        }
        else
        {
            // Disable virtual terminal and progress for non-output channels.
            m_vtOutputStream.Disable();

            // Disable progress for non-output channels
            m_spinner.reset();
            m_progressBar.reset();
            if (m_channel == Channel::Completion)
            {
                m_nonVTOutputStream.Enable();
            }
        }
    }

    BaseOutputStream& Reporter::GetOutputStream(Level level = Level::Info)
    {
        if (!IsVTEnabled() || m_channel == Channel::Completion)
        {
            return m_nonVTOutputStream;
        }

        switch (level)
        {
        case Level::Verbose:
            m_vtOutputStream.AddFormat(TextFormat::Default);
            break;
        case Level::Info:
            m_vtOutputStream.AddFormat(TextFormat::Default);
            break;
        case Level::Warning:
            m_vtOutputStream.AddFormat(TextFormat::Foreground::BrightYellow);
            break;
        case Level::Error:
            m_vtOutputStream.AddFormat(TextFormat::Foreground::BrightRed);
            break;
        default:
            THROW_HR(E_UNEXPECTED);
        }

        return m_vtOutputStream;
    }

    void Reporter::SetStyle(VisualStyle style)
    {
        m_style = style;
        if (m_spinner)
        {
            m_spinner->SetStyle(style);
        }
        if (m_progressBar)
        {
            m_progressBar->SetStyle(style);
        }
        if (style == VisualStyle::NoVT)
        {
            m_isVTEnabled = false;
        }
    }

    bool Reporter::PromptForBoolResponse(Resource::LocString message, Level level)
    {
        const std::vector<BoolPromptOption> options
        {
            BoolPromptOption{ Resource::String::PromptOptionYes, 'Y', true },
            BoolPromptOption{ Resource::String::PromptOptionNo, 'N', false },
        };

        auto out = GetOutputStream(level);
        out << message << std::endl;

        // Try prompting until we get a recognized option
        for (;;)
        {
            // Output all options
            for (size_t i = 0; i < options.size(); ++i)
            {
                out << PromptEmphasis << "[" + options[i].Hotkey.get() + "] " + options[i].Label.get();

                if (i + 1 == options.size())
                {
                    out << PromptEmphasis << ": ";
                }
                else
                {
                    out << "  ";
                }
            }

            // Read the response
            std::string response;
            if (!std::getline(m_in, response))
            {
                THROW_HR(APPINSTALLER_CLI_ERROR_PROMPT_INPUT_ERROR);
            }

            // Find the matching option ignoring whitespace
            Utility::Trim(response);
            for (const auto& option : options)
            {
                if (Utility::CaseInsensitiveEquals(response, option.Label) ||
                    Utility::CaseInsensitiveEquals(response, option.Hotkey))
                {
                    return option.Value;
                }
            }
        }
    }

    void Reporter::ShowIndefiniteProgress(bool running)
    {
        if (m_spinner)
        {
            if (running)
            {
                m_spinner->ShowSpinner();
            }
            else
            {
                m_spinner->StopSpinner();
            }
        }
    }

    void Reporter::OnProgress(uint64_t current, uint64_t maximum, ProgressType type)
    {
        ShowIndefiniteProgress(false);
        if (m_progressBar)
        {
            m_progressBar->ShowProgress(current, maximum, type);
        }
    }
    
    void Reporter::BeginProgress()
    {
        GetOutputStream() << VirtualTerminal::Cursor::Visibility::DisableShow;
        ShowIndefiniteProgress(true);
    };

    void Reporter::EndProgress(bool hideProgressWhenDone)
    {
        ShowIndefiniteProgress(false);
        if (m_progressBar)
        {
            m_progressBar->EndProgress(hideProgressWhenDone);
        }
        GetOutputStream() << VirtualTerminal::Cursor::Visibility::EnableShow;
    };

    void Reporter::SetProgressCallback(ProgressCallback* callback)
    {
        auto lock = m_progressCallbackLock.lock_exclusive();
        m_progressCallback = callback;
    }

    void Reporter::CancelInProgressTask(bool force)
    {
        // TODO: Maybe ask the user if they really want to cancel?
        UNREFERENCED_PARAMETER(force);
        auto lock = m_progressCallbackLock.lock_shared();
        ProgressCallback* callback = m_progressCallback.load();
        if (callback)
        {
            callback->Cancel();
        }
    }

    void Reporter::CloseOutputStream() 
    {
        GetOutputStream().Close();
    }

    bool Reporter::IsVTEnabled() const
    {
        return m_isVTEnabled && ConsoleModeRestore::Instance().IsVTEnabled();
    }
}
