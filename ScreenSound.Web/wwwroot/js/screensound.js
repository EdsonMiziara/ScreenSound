// =====================================================================
// Login no Spotify (Authorization Code + PKCE, sem client secret)
// https://developer.spotify.com/documentation/web-api/tutorials/code-pkce-flow
// =====================================================================
window.ssSpotify = (() => {
    const CHAVE_AUTH = 'ss-spotify-auth';
    const CHAVE_PKCE = 'ss-spotify-pkce';
    const ESCOPOS = 'streaming user-read-email user-read-private user-modify-playback-state user-read-playback-state';

    let clientId = null;
    let redirectUri = null;

    const base64Url = bytes => btoa(String.fromCharCode(...new Uint8Array(bytes)))
        .replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

    function aleatorio(tamanho) {
        const bytes = crypto.getRandomValues(new Uint8Array(tamanho));
        return base64Url(bytes).slice(0, tamanho);
    }

    function lerAuth() {
        try { return JSON.parse(localStorage.getItem(CHAVE_AUTH)); } catch { return null; }
    }

    function salvarAuth(resposta, anterior) {
        const auth = {
            access: resposta.access_token,
            refresh: resposta.refresh_token || anterior?.refresh,
            expira: Date.now() + (resposta.expires_in || 3600) * 1000
        };
        try { localStorage.setItem(CHAVE_AUTH, JSON.stringify(auth)); } catch { /* sem armazenamento */ }
        return auth;
    }

    async function pedirToken(parametros) {
        const resposta = await fetch('https://accounts.spotify.com/api/token', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ client_id: clientId, ...parametros })
        });
        if (!resposta.ok) throw new Error('Falha ao obter token do Spotify (' + resposta.status + ')');
        return resposta.json();
    }

    function configurar(id, redirect) {
        clientId = id || null;
        redirectUri = redirect || null;
    }

    const configurado = () => !!clientId && !!redirectUri;
    const conectado = () => configurado() && !!lerAuth()?.refresh;

    async function iniciarLogin(retorno) {
        if (!configurado()) return;
        const verifier = aleatorio(64);
        const estado = aleatorio(16);
        const hash = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(verifier));
        sessionStorage.setItem(CHAVE_PKCE, JSON.stringify({ verifier, estado, retorno }));

        const url = new URL('https://accounts.spotify.com/authorize');
        url.search = new URLSearchParams({
            response_type: 'code',
            client_id: clientId,
            scope: ESCOPOS,
            redirect_uri: redirectUri,
            code_challenge_method: 'S256',
            code_challenge: base64Url(hash),
            state: estado
        }).toString();
        window.location.assign(url.toString());
    }

    // Retorna a URL para onde voltar, ou lança erro com a mensagem para o usuário
    async function concluirLogin(query) {
        const params = new URLSearchParams(query);
        const pkce = JSON.parse(sessionStorage.getItem(CHAVE_PKCE) || 'null');
        sessionStorage.removeItem(CHAVE_PKCE);

        if (params.get('error')) throw new Error(params.get('error') === 'access_denied' ? 'Conexão com o Spotify cancelada.' : params.get('error'));
        if (!pkce || params.get('state') !== pkce.estado) throw new Error('Sessão de login do Spotify inválida. Tente novamente.');

        const resposta = await pedirToken({
            grant_type: 'authorization_code',
            code: params.get('code'),
            redirect_uri: redirectUri,
            code_verifier: pkce.verifier
        });
        salvarAuth(resposta);
        return pkce.retorno || '/';
    }

    async function token() {
        const auth = lerAuth();
        if (!auth || !clientId) return null;
        if (Date.now() < auth.expira - 60000) return auth.access;

        try {
            const resposta = await pedirToken({ grant_type: 'refresh_token', refresh_token: auth.refresh });
            return salvarAuth(resposta, auth).access;
        } catch {
            desconectar();
            return null;
        }
    }

    async function api(caminho, opcoes = {}) {
        const acesso = await token();
        if (!acesso) throw new Error('Spotify desconectado');
        return fetch('https://api.spotify.com/v1' + caminho, {
            ...opcoes,
            headers: { Authorization: 'Bearer ' + acesso, 'Content-Type': 'application/json', ...(opcoes.headers || {}) }
        });
    }

    function desconectar() {
        try { localStorage.removeItem(CHAVE_AUTH); } catch { /* sem armazenamento */ }
    }

    return { configurar, configurado, conectado, iniciarLogin, concluirLogin, token, api, desconectar };
})();

// =====================================================================
// Player global com dois motores:
//  - embed: iFrame API do Spotify (padrão, funciona para todos, sem volume)
//  - sdk:   Web Playback SDK (Spotify Premium, com volume)
// https://developer.spotify.com/documentation/embeds/references/iframe-api
// https://developer.spotify.com/documentation/web-playback-sdk
// =====================================================================
window.ssPlayer = (() => {
    let resolveEmbedApi;
    const embedApiPronta = new Promise(resolve => { resolveEmbedApi = resolve; });
    window.onSpotifyIframeApiReady = api => resolveEmbedApi(api);

    let resolveSdk;
    const sdkPronto = new Promise(resolve => { resolveSdk = resolve; });
    window.onSpotifyWebPlaybackSDKReady = () => resolveSdk();

    let modo = 'embed';
    let dotnet = null;

    // embed
    let controller = null;
    let querTocar = false;
    let tentativa = null;

    // sdk
    let sdk = null;
    let deviceId = null;
    let monitor = null;
    let volumeAtual = 0.7;

    // Detecção de fim de faixa: nenhum dos dois motores emite "ended", então observamos a posição
    let pertoDoFim = false;
    let terminou = false;
    let ultimoEnvio = 0;
    let ultimoPausado = null;

    function reiniciarEstado() {
        pertoDoFim = false;
        terminou = false;
        ultimoPausado = null;
    }

    function aoAtualizar(d) {
        const duracao = d.duration || 0;
        const posicao = d.position || 0;

        if (!d.isPaused) querTocar = false;

        if (duracao > 0 && posicao >= duracao - 1500) pertoDoFim = true;
        const voltouAoInicio = pertoDoFim && posicao < 1000;
        if (pertoDoFim && !terminou && (d.isPaused || voltouAoInicio)) {
            terminou = true;
            dotnet?.invokeMethodAsync('FaixaTerminou');
            return;
        }

        const agora = performance.now();
        if (agora - ultimoEnvio > 250 || d.isPaused !== ultimoPausado) {
            ultimoEnvio = agora;
            ultimoPausado = d.isPaused;
            dotnet?.invokeMethodAsync('AtualizarReproducao', !!d.isPaused, posicao, duracao, !!d.isBuffering);
        }
    }

    // ---------- motor embed ----------
    function agendarPlay() {
        clearTimeout(tentativa);
        tentativa = setTimeout(() => {
            if (querTocar && controller) controller.play();
        }, 1500);
    }

    async function carregarEmbed(hostId, uri) {
        const api = await embedApiPronta;

        if (!controller) {
            const host = document.getElementById(hostId);
            if (!host) return;
            host.innerHTML = '';
            const alvo = document.createElement('div');
            host.appendChild(alvo);

            await new Promise(resolve => {
                api.createController(alvo, { uri, width: '100%', height: 80 }, c => {
                    controller = c;
                    c.addListener('ready', () => {
                        if (querTocar && modo === 'embed') c.play();
                    });
                    c.addListener('playback_update', e => {
                        if (modo === 'embed') aoAtualizar(e.data || {});
                    });
                    resolve();
                });
            });
        } else {
            controller.loadUri(uri);
            controller.play();
        }

        agendarPlay();
    }

    // ---------- motor sdk ----------
    function carregarScriptSdk() {
        if (document.getElementById('spotify-sdk')) return;
        const script = document.createElement('script');
        script.id = 'spotify-sdk';
        script.src = 'https://sdk.scdn.co/spotify-player.js';
        script.async = true;
        document.head.appendChild(script);
    }

    function iniciarMonitor() {
        clearInterval(monitor);
        monitor = setInterval(async () => {
            if (modo !== 'sdk' || !sdk) return;
            const estado = await sdk.getCurrentState();
            if (!estado) return;
            aoAtualizar({ isPaused: estado.paused, position: estado.position, duration: estado.duration, isBuffering: estado.loading });
        }, 500);
    }

    function falharPremium(mensagem) {
        desativarPremium();
        dotnet?.invokeMethodAsync('AtualizarModo', false, null, mensagem);
    }

    let ativando = null;

    // Pode ser chamado pelo layout e pela página de callback ao mesmo tempo: só cria um player
    function ativarPremium(referencia, volume) {
        dotnet = referencia;
        volumeAtual = typeof volume === 'number' ? volume : volumeAtual;
        if (sdk && deviceId) {
            modo = 'sdk';
            dotnet?.invokeMethodAsync('AtualizarModo', true, null, null);
            return Promise.resolve();
        }
        if (!ativando) ativando = criarPlayerSdk().finally(() => { ativando = null; });
        return ativando;
    }

    async function criarPlayerSdk() {
        if (sdk) return;
        carregarScriptSdk();
        await sdkPronto;

        sdk = new Spotify.Player({
            name: 'ScreenSound Web',
            getOAuthToken: cb => window.ssSpotify.token().then(t => cb(t || '')),
            volume: volumeAtual
        });

        sdk.addListener('ready', async ({ device_id }) => {
            deviceId = device_id;
            modo = 'sdk';
            controller?.pause();
            iniciarMonitor();

            let nome = null;
            try {
                const perfil = await (await window.ssSpotify.api('/me')).json();
                nome = perfil.display_name || perfil.id;
            } catch { /* nome é opcional */ }
            dotnet?.invokeMethodAsync('AtualizarModo', true, nome, null);
        });
        sdk.addListener('not_ready', () => { deviceId = null; });
        sdk.addListener('account_error', () => falharPremium('O controle de volume exige uma conta Spotify Premium.'));
        sdk.addListener('authentication_error', () => {
            window.ssSpotify.desconectar();
            falharPremium('Sua sessão do Spotify expirou. Conecte novamente.');
        });
        sdk.addListener('initialization_error', () => falharPremium('Este navegador não suporta o player do Spotify.'));

        const conectou = await sdk.connect();
        if (!conectou) falharPremium('Não foi possível conectar ao Spotify.');
    }

    function desativarPremium() {
        clearInterval(monitor);
        if (sdk) {
            sdk.disconnect();
            sdk = null;
        }
        deviceId = null;
        modo = 'embed';
    }

    async function carregarSdk(uri) {
        sdk.activateElement?.();
        const corpo = uri.startsWith('spotify:track:') || uri.startsWith('spotify:episode:')
            ? { uris: [uri] }
            : { context_uri: uri };

        const resposta = await window.ssSpotify.api(`/me/player/play?device_id=${encodeURIComponent(deviceId)}`, {
            method: 'PUT',
            body: JSON.stringify(corpo)
        });
        if (!resposta.ok && resposta.status !== 204) {
            falharPremium('O Spotify recusou a reprodução (' + resposta.status + '). Voltando ao player padrão.');
        }
    }

    // ---------- interface usada pelo PlayerService ----------
    async function load(hostId, uri, referencia) {
        dotnet = referencia;
        querTocar = true;
        reiniciarEstado();

        if (modo === 'sdk' && sdk && deviceId) {
            // Se o SDK falhar, falharPremium avisa o .NET, que recarrega a faixa no embed
            await carregarSdk(uri);
            return;
        }

        await carregarEmbed(hostId, uri);
    }

    function toggle() {
        if (modo === 'sdk' && sdk) sdk.togglePlay();
        else controller?.togglePlay();
    }

    function seek(segundos) {
        pertoDoFim = false;
        terminou = false;
        if (modo === 'sdk' && sdk) sdk.seek(Math.round(segundos * 1000));
        else controller?.seek(segundos);
    }

    function setVolume(valor) {
        volumeAtual = Math.min(1, Math.max(0, valor));
        if (modo === 'sdk' && sdk) sdk.setVolume(volumeAtual);
    }

    function destroy() {
        clearTimeout(tentativa);
        querTocar = false;
        if (sdk) sdk.pause();
        if (controller) {
            controller.destroy();
            controller = null;
        }
    }

    return { load, toggle, seek, setVolume, destroy, ativarPremium, desativarPremium };
})();

window.ssStorage = {
    get: chave => {
        try { return localStorage.getItem(chave); } catch { return null; }
    },
    set: (chave, valor) => {
        try { localStorage.setItem(chave, valor); } catch { /* armazenamento indisponível */ }
    }
};

window.ssUi = {
    // Fração (0..1) da posição horizontal do clique dentro do elemento — usada para buscar na waveform
    fracao: (elemento, clientX) => {
        if (!elemento) return 0;
        const r = elemento.getBoundingClientRect();
        return r.width > 0 ? Math.min(1, Math.max(0, (clientX - r.left) / r.width)) : 0;
    },
    copiar: async texto => {
        try {
            await navigator.clipboard.writeText(texto);
            return true;
        } catch {
            return false;
        }
    },
    topo: () => window.scrollTo({ top: 0, behavior: 'instant' })
};
