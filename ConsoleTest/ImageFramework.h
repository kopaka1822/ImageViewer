#pragma once
#include "Pipeline.h"
#include <string>

namespace ImageFramework
{
	class Model
	{
	public:
		struct Int2
		{
			int x;
			int y;
		};

		struct FilterParam
		{
			std::string name;
			std::string value;
		};

		struct Statistic
		{
			float luminance;
			float lightness;
			float luma;
		};

		struct StatisticModel
		{
			Statistic min;
			Statistic max;
			Statistic avg;
		};

		struct PixelColor
		{
			struct
			{
				float r, g, b, a;
			} linear;
			struct
			{
				uint8_t r, g, b, a;
			} srgb;
		};

		/// \param consolePath location of ImageConsole.exe
		Model(const std::string& consolePath = "ImageConsole.exe");

		~Model();

		/// opens image
		void OpenImage(std::string_view filename);
		/// deletes image
		void DeleteImage(int index);
		/// deletes all images
		void ClearImages();
		/// moves image to a new index
		void MoveImage(int oldIndex, int newIndex);
		/// sets image combine equation
		void SetEquation(std::string_view color, std::string_view alpha = std::string_view());

		/// add filter to the filter list
		void OpenFilter(std::string_view filename);
		/// deletes filter from filter list
		void DeleteFilter(int index);
		/// deletes all filter
		void ClearFilter();

		/// returns all parameters with current set values
		std::vector<FilterParam> GetFilterParams(int filterIndex) const;
		/// sets filter parameter of the filter at filterIndex
		void SetFilterParam(int filterIndex, std::string_view paramName, std::string_view value);

		int GetNumLayers() const;
		int GetNumMipmaps() const;
		/// generates mipmaps or overwrites existing ones
		void GenMipmaps();
		/// deletes all mipmaps
		void DeleteMipmaps();
		/// returns size of the mipmap
		Int2 GetSize(int mipmap) const;
		bool IsAlpha() const;
		StatisticModel GetStatistics() const;
		PixelColor GetPixelColor(int x, int y, int layer = 0, int mipmap = 0, int radius = 0);

		void SetExportLayer(int layer);
		void SetExportMipmap(int mipmap);
		void SetExportQuality(int quality);
		void SetExportCropping(int xStart, int yStart, int xEnd, int yEnd);
		void DisableExportCropping();
		void Export(std::string_view filename, std::string_view format) const;
		/// returns all supported formats for a file extension (png, jpg, ...)
		std::vector<std::string> GetExportFormats(std::string_view extension) const;

		/// waits for all console input to be finished
		void Sync() const;

		/// generates a thumbnail with the specified size
		/// \param size max width/height
		/// \param dstWidth (out) width of the generated thumbnail
		/// \param dstHeight (out) height of the generated thumbnail
		std::vector<uint8_t> GenThumbnail(int size, int& dstWidth, int& dstHeight);
	private:
		std::string ReadLine() const
		{
			// wait at most 10 seconds
			for(int i = 0; i < 10000; ++i)
			{
				if (m_err.CanRead())
					throw std::runtime_error(m_err.ReadLine());

				if (m_out.CanRead())
					return m_out.ReadLine();

				//Yield();
				Sleep(1);
			}
			throw std::runtime_error("read line timeout expired");
		}

		std::vector<uint8_t> ReadBinary(DWORD numBytes)
		{
			return m_out.ReadBinary(numBytes);
		}

		Statistic GetStatistic(std::string_view name) const
		{
			m_in.Write("-stats ");
			m_in.Write(name);
			m_in.Write("\n");

			const auto luminance = ReadLine();
			const auto lightness = ReadLine();
			const auto luma = ReadLine();

			Statistic stat;
			stat.luminance = GetLastFloat(luminance);
			stat.lightness = GetLastFloat(lightness);
			stat.luma = GetLastFloat(luma);
			return stat;
		}

		static float GetLastFloat(const std::string& text)
		{
			const auto lastIdx = text.find_last_of(' ');
			if (lastIdx == std::string::npos) return std::stof(text);
			return std::stof(text.substr(lastIdx + 1));
		}

		template<size_t len>
		std::array<float, len> GetFloats(std::string text)
		{
			std::array<float, len> res;
			auto i = res.begin();

			size_t numRead = 0;
			while (i != res.end())
			{
				if (numRead) text = text.substr(numRead);
				*i++ = std::stof(text, &numRead);
			}

			return res;
		}
	private:
		PROCESS_INFORMATION m_info;
		mutable detail::Pipeline m_in;
		detail::Pipeline m_out;
		detail::Pipeline m_err;
	};

	inline Model::Model(const std::string& consolePath) :
	m_info({}),
		m_in(detail::Pipeline::StdIn), m_out(detail::Pipeline::StdOut), m_err(detail::Pipeline::StdOut)
	{
		STARTUPINFOA st;
		ZeroMemory(&st, sizeof(st));
		st.cb = sizeof(st);
		st.hStdError = m_err.GetWrite();
		st.hStdOutput = m_out.GetWrite();
		st.hStdInput = m_in.GetRead();
		st.dwFlags = STARTF_USESTDHANDLES;
		

		char args[] = "ImageConsole.exe -cin -silent";
		
		if (!CreateProcessA(
			consolePath.c_str(),
			args,
			nullptr,
			nullptr,
			TRUE,
			NORMAL_PRIORITY_CLASS | CREATE_NO_WINDOW,
			nullptr,
			nullptr,
			&st,
			&m_info
		)) throw std::runtime_error("could not launch ImageConsole.exe");
	}

	inline Model::~Model()
	{
		try
		{
			m_in.Write("-close\n");
			m_in.Flush();
			TerminateProcess(m_info.hProcess, 0);
			WaitForSingleObject(m_info.hProcess, 10000);
		}
		catch (...)
		{}

		CloseHandle(m_info.hProcess);
		CloseHandle(m_info.hThread);
	}

	inline void Model::OpenImage(std::string_view filename)
	{
		m_in.Write("-open \"");
		m_in.Write(filename);
		m_in.Write("\"\n");
	}

	inline void Model::DeleteImage(int index)
	{
		m_in.Write("-delete ");
		m_in.Write(std::to_string(index));
		m_in.Write("\n");
	}

	inline void Model::ClearImages()
	{
		m_in.Write("-delete\n");
	}

	inline void Model::MoveImage(int oldIndex, int newIndex)
	{
		m_in.Write("-move ");
		m_in.Write(std::to_string(oldIndex));
		m_in.Write(" ");
		m_in.Write(std::to_string(newIndex));
		m_in.Write("\n");
	}

	inline void Model::OpenFilter(std::string_view filename)
	{
		m_in.Write("-addfilter \"");
		m_in.Write(filename);
		m_in.Write("\"\n");
	}

	inline void Model::DeleteFilter(int index)
	{
		m_in.Write("-deletefilter ");
		m_in.Write(std::to_string(index));
		m_in.Write("\n");
	}

	inline void Model::ClearFilter()
	{
		m_in.Write("-deletefilter\n");
	}

	inline std::vector<Model::FilterParam> Model::GetFilterParams(int filterIndex) const
	{
		m_in.Write("-tellfilterparams ");
		m_in.Write(std::to_string(filterIndex));
		m_in.Write("\n");

		std::vector<FilterParam> res;

		std::string line;
		while (!(line = ReadLine()).empty())
		{
			auto split = line.find_last_of(' ');
			res.emplace_back();
			res.back().name = line.substr(0, split);
			res.back().value = line.substr(split + 1);
		}

		return res;
	}

	inline void Model::SetFilterParam(int filterIndex, std::string_view paramName, std::string_view value)
	{
		m_in.Write("-filterparam ");
		m_in.Write(std::to_string(filterIndex));
		m_in.Write(" \"");
		m_in.Write(paramName);
		m_in.Write("\" ");
		m_in.Write(value);
		m_in.Write("\n");
	}

	inline void Model::SetEquation(std::string_view color, std::string_view alpha)
	{
		m_in.Write("-equation \"");
		m_in.Write(color);
		if (alpha.data())
		{
			m_in.Write("\" \"");
			m_in.Write(alpha);
		}
		m_in.Write("\"\n");
	}

	inline int Model::GetNumLayers() const
	{
		m_in.Write("-telllayers\n");
		return std::stol(ReadLine());
	}

	inline int Model::GetNumMipmaps() const
	{
		m_in.Write("-tellmipmaps\n");
		return std::stol(ReadLine());
	}

	inline void Model::GenMipmaps()
	{
		m_in.Write("-genmipmaps\n");
	}

	inline void Model::DeleteMipmaps()
	{
		m_in.Write("-deletemipmaps\n");
	}

	inline Model::Int2 Model::GetSize(int mipmap) const
	{
		m_in.Write("-tellsize ");
		m_in.Write(std::to_string(mipmap));
		m_in.Write("\n");

		Int2 res;
		res.x = std::stol(ReadLine());
		res.y = std::stol(ReadLine());
		return res;
	}

	inline bool Model::IsAlpha() const
	{
		m_in.Write("-tellalpha\n");
		return ReadLine() == "True";
	}

	inline Model::StatisticModel Model::GetStatistics() const
	{
		StatisticModel m;
		m.min = GetStatistic("min");
		m.max = GetStatistic("max");
		m.avg = GetStatistic("avg");

		return m;
	}

	inline Model::PixelColor Model::GetPixelColor(int x, int y, int layer, int mipmap, int radius)
	{
		m_in.Write("-tellpixel ");
		m_in.Write(std::to_string(x));
		m_in.Write(" ");
		m_in.Write(std::to_string(y));
		if (layer != 0 || mipmap != 0 || radius != 0)
		{
			m_in.Write(" ");
			m_in.Write(std::to_string(layer));
			m_in.Write(" ");
			m_in.Write(std::to_string(mipmap));
			m_in.Write(" ");
			m_in.Write(std::to_string(radius));
		}
		m_in.Write("\n");

		auto linear = ReadLine();
		auto srgb = ReadLine();
		const auto linVal = GetFloats<4>(move(linear));
		const auto byteVal = GetFloats<4>(move(srgb));

		PixelColor res;
		res.linear.r = linVal[0];
		res.linear.g = linVal[1];
		res.linear.b = linVal[2];
		res.linear.a = linVal[3];

		res.srgb.r = static_cast<uint8_t>(byteVal[0]);
		res.srgb.g = static_cast<uint8_t>(byteVal[1]);
		res.srgb.b = static_cast<uint8_t>(byteVal[2]);
		res.srgb.a = static_cast<uint8_t>(byteVal[3]);

		return res;
	}

	inline void Model::SetExportLayer(int layer)
	{
		m_in.Write("-exportlayer ");
		m_in.Write(std::to_string(layer));
		m_in.Write("\n");
	}

	inline void Model::SetExportMipmap(int mipmap)
	{
		m_in.Write("-exportmipmap ");
		m_in.Write(std::to_string(mipmap));
		m_in.Write("\n");
	}

	inline void Model::SetExportQuality(int quality)
	{
		m_in.Write("-exportquality ");
		m_in.Write(std::to_string(quality));
		m_in.Write("\n");
	}

	inline void Model::SetExportCropping(int xStart, int yStart, int xEnd, int yEnd)
	{
		m_in.Write("-exportcrop true ");
		m_in.Write(std::to_string(xStart));
		m_in.Write(" ");
		m_in.Write(std::to_string(yStart));
		m_in.Write(" ");
		m_in.Write(std::to_string(xEnd));
		m_in.Write(" ");
		m_in.Write(std::to_string(yEnd));
		m_in.Write("\n");
	}

	inline void Model::DisableExportCropping()
	{
		m_in.Write("-exportcrop false\n");
	}

	inline void Model::Export(std::string_view filename, std::string_view format) const
	{
		m_in.Write("-export ");
		m_in.Write(filename);
		m_in.Write(" ");
		m_in.Write(format);
		m_in.Write("\n");
	}

	inline std::vector<std::string> Model::GetExportFormats(std::string_view extension) const
	{
		m_in.Write("-tellformats ");
		m_in.Write(extension);
		m_in.Write("\n");

		std::vector<std::string> res;

		std::string line;
		while (!(line = ReadLine()).empty())
		{
			res.emplace_back(std::move(line));
		}

		return res;
	}

	inline void Model::Sync() const
	{
		// execute simple command where the client needs to respond
		m_in.Write("-telllayers\n");
		ReadLine();
	}

	inline std::vector<uint8_t> Model::GenThumbnail(int size, int& dstWidth, int& dstHeight)
	{
		m_in.Write("-thumbnail ");
		m_in.Write(std::to_string(size));
		m_in.Write("\n");

		dstWidth = std::stol(ReadLine());
		dstHeight = std::stol(ReadLine());
		return ReadBinary(dstWidth * dstHeight * 4);
	}
}

