
export interface JsonError {
    code: number;
    message: string;
    data: any;
}

export interface JsonResponse<T> {
    id: any;
    result: T;
    error: JsonError;
}

type FuncCallback<T> = (result: T, jResponse: JsonResponse<T>) => void;
type ActionCallback = (jResponse: JsonResponse<void>) => void;
type FailCallback<T> = (error: JsonError, jResponse: JsonResponse<T>) => void;

// Remote method in server.
abstract class RemoteMethod<T> {
    private static nextId: number = 1;
    private failCallback: FailCallback<T> = null;
    private unknownError: JsonError = { code: null, message: 'Unknown error.', data: null };
    
    fail(callback: FailCallback<T>): RemoteMethod<T>{
        this.failCallback = callback;
        return this;
    }

    protected abstract resolve(jResponse: JsonResponse<T>);

    private reject(jResponse: JsonResponse<T> = null) {
        if (this.failCallback) {
            if (jResponse) this.failCallback(jResponse.error, jResponse);
            else this.failCallback(this.unknownError, null);
        }
    }

    static callFunc<T>(method, args): RemoteFunc<T> {
        return RemoteMethod.call<T>(new RemoteFunc<T>(), method, args) as RemoteFunc<T>;
    }

    static callAction(method, args): RemoteAction {
        return RemoteMethod.call<void>(new RemoteAction(), method, args) as RemoteAction;
    }

    private static call<T>(remoteMethod: RemoteMethod<T>, method: string, args): RemoteMethod<T> {
        let parameters = [];
        for (var i in args) parameters.push(args[i]);
        let jsonRequest = JSON.stringify({ method: method, params: parameters, id: RemoteMethod.nextId });
        RemoteMethod.nextId++;

        let request = new XMLHttpRequest();
        request.open('POST', '/typedrpc', true);

        request.onload = function () {
            if (request.status >= 200 && request.status < 400) {
                let jResponse = JSON.parse(request.responseText);
                if (jResponse.error) {
                    remoteMethod.reject(jResponse);
                } else {
                    remoteMethod.resolve(jResponse);
                }
            }
            else {
                remoteMethod.reject();
            }
        };

        request.onerror = function () {
            remoteMethod.reject();
        };

        request.send(jsonRequest);

        return remoteMethod;
    }
}

// A Func returns a value.
export class RemoteFunc<T> extends RemoteMethod<T> {
    private funcCallback: FuncCallback<T> = null;

    done(callback: FuncCallback<T>): RemoteFunc<T> {
        this.funcCallback = callback;
        return this;
    }

    protected resolve(jResponse: JsonResponse<T>) {
        if (this.funcCallback) this.funcCallback(jResponse.result, jResponse);
    }
}

// An action doesn't return a value.
export class RemoteAction extends RemoteMethod<void> {
    private actionCallback: ActionCallback = null;

    done(callback: ActionCallback): RemoteAction {
        this.actionCallback = callback;
        return this;
    }

    protected resolve(jResponse: JsonResponse<void>) {
        if (this.actionCallback) this.actionCallback(jResponse);
    }
}
