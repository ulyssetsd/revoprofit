import React, { MouseEventHandler, ChangeEventHandler } from "react";
import Accordion from "./components/Accordion"

const clickHandler: MouseEventHandler = event => {
    
}

const handleChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    var selectorFiles = e.target.files;
    console.log(selectorFiles);
}

function App() {
    return (
        <div className="container">
            <h1 className="text-center m-5">RevoProfit</h1>
            <p className="text-center m-5">RevoProfit is not affiliated or associated any way with Revolut™ Ltd. The realized gain calculation is based on the Average Cost methodology. You should consult an accountant to validate the results and make sure they comply with your local tax authority rules.</p>
            <div className="mb-3">
                <label htmlFor="formFile" className="form-label">Your Revolut export file (.csv)</label>
                <input className="form-control" type="file" name="formFile" id="formFile" required onChange={handleChange}/>
            </div>
            <button className="btn btn-primary btn-lg" onClick={clickHandler}>💸 See Results</button>
            <Accordion items={[
                {
                    title: "How do we calculate realized gains?",
                    subtitle: "We are using the Average Cost Basis Method. This is the method required by the French government for realized gains calculation."
                },
                {
                    title: "Do you store my trading data?",
                    subtitle: "No, we do not store any personal data, trading data, or cookies."
                },
                {
                    title: "How can I get my Revolut report in .csv format?",
                    subtitle: 'Open your Revolut app and go to the "Stocks" section. Click on the "..." button and select "Statement" > "Account Statement". Choose the Excel format, and choose the dates from June 2017 to latest month.'
                }
            ]} />
        </div>
    );
}

export default App;