
class RemoteMethod {
    private static nextId: number = 1;
    private doneCallback = null;
    private failCallback = null;

    done(callback): RemoteMethod {
        this.doneCallback = callback;
        return this;
    }
    
    fail(callback): RemoteMethod {
        this.failCallback = callback;
        return this;
    }

    private resolve(jResponse) {
        if (this.doneCallback) this.doneCallback(jResponse.result, jResponse);
    }

    private reject(jResponse = null) {
        if (this.failCallback) {
            if (jResponse) this.failCallback(jResponse.error, jResponse);
            else this.failCallback({ message: 'Unknown error.' }, null);
        }
    }

    static call<T>(method, args) {
        var parameters = [];
        for (var i in args) parameters.push(args[i]);
        var jsonRequest = JSON.stringify({ method: method, params: parameters, id: RemoteMethod.nextId });
        RemoteMethod.nextId++;
        var remoteMethod = new RemoteMethod();

        var request = new XMLHttpRequest();
        request.open('POST', '/typedrpc', true);

        request.onload = function () {
            if (request.status >= 200 && request.status < 400) {
                var jResponse = JSON.parse(request.responseText);
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
