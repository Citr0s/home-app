import { environment } from '../../environments/environment';
import { WebSocketKey } from './types/web-socket.key';
import { Stack } from '../../core/stack';
import { Subject } from 'rxjs';

export class WebSocketService {

    private static _INSTANCE: WebSocketService | null = null;

    public static instance(): WebSocketService {
        if (this._INSTANCE === null) {
            this._INSTANCE = new WebSocketService();
        }
        return this._INSTANCE;
    }

    public isConnected: Subject<boolean> = new Subject<boolean>();

    private _sessionId: string | null = localStorage.getItem('sessionId');
    private _deployOngoing: boolean = false;
    private _isReady: boolean | null = null;
    private _queue: Stack<any> = new Stack<any>();
    private _webSocket: WebSocket | null = null;
    private _subscribers: Map<WebSocketKey, Array<(payload: any) => void>> = new Map<WebSocketKey, Array<(payload: any) => void>>();

    private constructor() {
        try {
            this._webSocket = new WebSocket(`${environment.webSocketUrl}/ws`);

            this._webSocket.onopen = () => {
                this.handleOpen();
            };
            this._webSocket.onmessage = (e: any) => {
                this.handleMessage(e);
            };
            this._webSocket.onclose = () => {
                this.handleClose();
            };
            this._webSocket.onerror = (e: any) => {
                this.handleError(e);
            };

            setInterval(() => {
                if (this._queue.size() === 0) {
                    return;
                }
                const queuedRequest = this._queue.pop();
                this.send(queuedRequest.Key, queuedRequest.Data);
            }, 1000);
        } catch (e) {
            console.log('Failed to initialise WebSocket connection...');
            console.log(e);
        }
    }

    public subscribe(key: WebSocketKey, callback: (payload: any) => void): void {
        if (this._subscribers.has(key)) {
            const existingCallbacks = this._subscribers.get(key);

            if (existingCallbacks) {
                existingCallbacks.push(callback);
                this._subscribers.set(key, existingCallbacks);
            }

            return;
        }

        this._subscribers.set(key, [callback]);
    }

    public unsubscribe(payload: any): void {
        this._webSocket?.send(JSON.stringify(payload));
    }

    public send(key: WebSocketKey, payload: any): void {
        if (this._isReady) {
            this._webSocket?.send(JSON.stringify({Key: key, Data: payload, SessionId: this._sessionId}));
        } else {
            this._queue.push({Key: key, Data: payload});
        }
    }

    public handleOpen(): void {
        console.log('WebSocket connection is open...');
        this._isReady = true;
        this.isConnected.next(this._isReady);
    }

    public handleMessage(e: any): void {
        console.log('WebSocket message received.');

        const response = JSON.parse(e.data);

        if (response.Key === WebSocketKey.Handshake) {
            console.log('Received', WebSocketKey.Handshake, response.Data);
            this._sessionId = response.Data;
            localStorage.setItem('sessionId', response.Data);
        } else {
            console.log('Received', response.Key, response.Data);
            for (const callback of this._subscribers.get(response.Key) ?? []) {
                callback(response.Data);
            }
        }

        if (response.Key === WebSocketKey.DeployStarted || response.Key === WebSocketKey.DeployUpdated) {
            this._deployOngoing = true;
        }
    }

    public handleClose(): void {
        console.log('WebSocket connection is closed...');
        this._isReady = false;
        this.isConnected.next(this._isReady);

        if (this._deployOngoing) {
            setTimeout(() => {
                console.log('Refreshing site in 5 seconds...');
                location.reload();
            }, 5000);
        }
    }

    public handleError(error: any): void {
        console.log('WebSocket error occurred...');
        console.log(error);
    }
}