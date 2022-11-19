declare var require: any

var React = require('react');
var ReactDOM = require('react-dom');
var fetch = require("cross-fetch");

fetch("/quote", {
    method: "GET"
}).then(resp => resp.text())
    .then(data => {
        const elm = <span className="lead font-italic">"{data}"</span>
        ReactDOM.render(elm, document.getElementById('root'));
    });

export class Quote extends React.Component {
    render() {
        return (
            <span className="lead font-italic">""</span>
        );
    }
}

ReactDOM.render(<Quote />, document.getElementById('root'));