using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VRChatAPI.Models
{
    public class HttpFactory
    {
        private static readonly HttpMethod _patchMethod = new HttpMethod("PATCH");
        private readonly HttpClient _httpClient;

        public static HttpMethod Patch
        {
            get => _patchMethod;
        }

        public bool DebugHttp { get; set; } = false;

        public HttpFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TJson> GetAsync<TJson>(string uri, CancellationToken? ct = null) where TJson : class
        {
            try
            {
                using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct ?? CancellationToken.None).ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (!response.IsSuccessStatusCode)
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                response.EnsureSuccessStatusCode();
                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);
                return JsonSerializer.Create().Deserialize<TJson>(jsonReader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<HttpResponseMessage> GetResponseAsync(string uri, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken? ct = null)
        {
            try
            {
                var ret = await _httpClient.GetAsync(uri, completionOption, ct ?? CancellationToken.None).ConfigureAwait(false);
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<string> GetStringAsync(string uri, CancellationToken? ct = null)
        {
            try
            {
                using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct ?? CancellationToken.None).ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new StreamReader(contentStream);
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _httpClient.DefaultRequestHeaders.Remove("Content-Type");
            }

            return string.Empty;
        }

        public async Task<string> PutStringAsync(string uri, HttpContent content, HttpClient awsHttpClient = null, CancellationToken? ct = null)
        {
            try
            {
                awsHttpClient = _httpClient;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                using var request = new HttpRequestMessage(HttpMethod.Put, uri) { Content = content };
                using var response = await awsHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct ?? CancellationToken.None).ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new StreamReader(contentStream);
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return string.Empty;
        }

        public async Task<TJson> PostAsync<TJson>(string uri, HttpContent content, CancellationToken? ct = null) where TJson : class
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct ?? CancellationToken.None).ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (!response.IsSuccessStatusCode)
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                response.EnsureSuccessStatusCode();
                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);
                return JsonSerializer.Create().Deserialize<TJson>(jsonReader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
    }

    public class ProgressableByteArrayContent : HttpContent
    {
        private byte[] _content;
        private int _offset;
        private int _count;
        private IProgress<double> _progress;
        private int _chunkSize;

        public ProgressableByteArrayContent(byte[] content, IProgress<double> progress, int chunkSize = 131072) : this(content, 0, content.Length, progress, chunkSize)
        {

        }

        public ProgressableByteArrayContent(byte[] content, int offset, int count, IProgress<double> progress, int chunkSize = 131072)
        {
            _content = content;
            _offset = offset;
            _count = count;
            _progress = progress;
            _chunkSize = ChangeChunkSizeIfNecessary(chunkSize);
        }

        private int ChangeChunkSizeIfNecessary(int chunkSize)
        {
            return _count < chunkSize ? _count : chunkSize;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            long written = 0;
            for (int i = 0; i < _content.Length; i += _chunkSize)
            {
                int count = Math.Min(_chunkSize, _content.Length - i);

                written += count;

                _progress.Report(Math.Round((double)written / _count * 100, 2));

                await stream.WriteAsync(_content, i, count).ConfigureAwait(false);
            }

            _progress.Report(Math.Round((double)written / _count * 100, 2));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _count;
            return true;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>((Stream)new MemoryStream(_content, _offset, _count, false, false));
        }
    }

    public class ProgressableStreamContent : HttpContent
    {
        private Stream _content;
        private long _start;
        private IProgress<double> _progress;
        private int _chunkSize;
        private bool _contentConsumed = false;

        public ProgressableStreamContent(Stream content, IProgress<double> progress, int chunkSize = 131072)
        {
            _content = content;
            if (_content.CanSeek)
                _start = _content.Position;
            _progress = progress;
            _chunkSize = chunkSize;
        }

        private int ChangeChunkSizeIfNecessary(int chunkSize)
        {
            return _content.Length < chunkSize ? (int)_content.Length : chunkSize;
        }

        private void PrepareContent()
        {
            if (_contentConsumed)
                if (!_content.CanSeek)
                    _content.Position = _start;
            _contentConsumed = true;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            PrepareContent();
            var buffer = new byte[_chunkSize];
            long written = 0;
            int chunk = 0;
            while ((chunk = await _content.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                written += chunk;

                _progress.Report(Math.Round((double)written / _content.Length * 100, 2));

                await stream.WriteAsync(buffer, 0, chunk).ConfigureAwait(false);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            if (_content.CanSeek)
            {
                length = _content.Length - _start;
                return true;
            }
            length = 0L;
            return false;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(_content);
        }
    }

    public class ProgressableStringContent : ProgressableByteArrayContent
    {
        private const string _defaultMediaType = "text/plain";


        public ProgressableStringContent(string content, Encoding encoding, string mediaType, IProgress<double> progress) : base(GetContentByteArray(content, encoding), progress)
        {
            Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(mediaType) ? _defaultMediaType : mediaType)
            {
                CharSet = encoding == null ? Encoding.UTF8.WebName : encoding.WebName
            };
        }

        private static byte[] GetContentByteArray(string content, Encoding encoding)
        {
            encoding = Encoding.UTF8;
            return encoding.GetBytes(content);
        }
    }

    public class ProgressableJsonContent : ProgressableStringContent
    {
        public ProgressableJsonContent(string content, IProgress<double> progress) : base(content, Encoding.UTF8, "application/json", progress)
        {

        }
    }

    public class JsonContent : StringContent
    {
        public JsonContent(string content) : base(content, Encoding.UTF8, "application/json")
        {

        }
    }
}
